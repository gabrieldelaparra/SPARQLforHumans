using System;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    [Collection("Sequential")]
    public class QueryGraphTypesTests : IDisposable
    {
        public QueryGraphTypesTests()
        {
            const string filename = @"Resources/QueryGraphTypes.nt";
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, EntitiesIndexPath).Index();
            new PropertiesIndexer(filename, PropertiesIndexPath, EntitiesIndexPath).Index();
        }

        public void Dispose()
        {
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
        }

        private const string EntitiesIndexPath = "QueryGraphTypesEntities";
        private const string PropertiesIndexPath = "QueryGraphTypesProperties";


        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// 
        /// Expected Results:
        /// ?var1 are entities coming from Obama
        /// ?prop0 are properties coming from Obama
        /// </summary>
        [Fact]
        public void TestTypes_2ConnectedNodes_1_SourceIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0", new[] {"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1")
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1)
                }
            };
            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Types);
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes); //InstanceOf Q76: Q5
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes);

            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q76", queryGraph.Nodes[0].Types);

            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[1].InferredTypes);
            Assert.Empty(queryGraph.Nodes[1].ParentTypes);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes); //InstanceOf Q76: Q5
            Assert.Empty(queryGraph.Edges[0].RangeTypes);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// 
        /// Expected Results:
        /// ?var1 are entities coming from Obama
        /// ?prop0 are properties coming from Obama
        /// </summary>
        [Fact]
        public void TestTypes_2ConnectedNodes_1_TargetIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q76"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1)
                }
            };
            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[0].InferredTypes);
            Assert.Empty(queryGraph.Nodes[0].ParentTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].Types);
            Assert.NotEmpty(queryGraph.Nodes[1].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q76", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Edges[0].DomainTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes);
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// ?var1 is USA
        /// 
        /// Expected Results:
        /// ?prop0 are properties from Obama going to USA
        /// </summary>
        [Fact]
        public void TestTypes_2ConnectedNodes_2_SourceIsGivenType_TargetIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0", new[] {"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q30"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1)
                }
            };
            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Types);
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes);

            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q76", queryGraph.Nodes[0].Types);

            Assert.NotEmpty(queryGraph.Nodes[1].Types);
            Assert.NotEmpty(queryGraph.Nodes[1].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q30", queryGraph.Nodes[1].Types);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes);
        }

        /// <summary>
        /// ?var0 P27 ?var1
        /// ?var0 is Obama
        /// ?var1 is USA
        /// 
        /// Expected Results:
        /// All is given, should behave like that.
        /// </summary>
        [Fact]
        public void TestTypes_2ConnectedNodes_3_SourceIsGivenType_TargetIsGivenType_E0N0P27E1_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0", new[] {"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q30"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P27"})
                }
            };
            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Types); //Q76
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes); //Domain P27
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes);

            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q76", queryGraph.Nodes[0].Types);
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);

            Assert.NotEmpty(queryGraph.Nodes[1].Types); //Q30
            Assert.NotEmpty(queryGraph.Nodes[1].InferredTypes); //Range P27
            Assert.NotEmpty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q30", queryGraph.Nodes[1].Types);
            Assert.Single(queryGraph.Nodes[1].InferredTypes);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// </summary>
        [Fact]
        public void TestTypes_2ConnectedNodes_4_SubjectIsInstanceOfType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q5"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P31"})
                }
            };
            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[0].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes); //InstanceOf Q5

            Assert.Single(queryGraph.Nodes[0].ParentTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].ParentTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[1].InferredTypes);
            Assert.Empty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes); //Range of P31

            Assert.Single(queryGraph.Edges[0].DomainTypes); //?human is type Q5. Domain should be Q5.
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var0 ?prop0 ?var2
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are
        /// TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with domain in Human 
        /// </summary>
        [Fact]
        public void TestTypes_3ConnectedNodes_1_N0InstanceOfN1_E1DomainN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2")
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 0, 2)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes); //InstanceOf Q5

            Assert.Single(queryGraph.Nodes[0].ParentTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].ParentTypes);
            Assert.Single(queryGraph.Nodes[0].InferredTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[1].InferredTypes);
            Assert.Empty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Nodes[2].InferredTypes);
            Assert.Empty(queryGraph.Nodes[2].ParentTypes);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes); //Range of P31

            Assert.Single(queryGraph.Edges[0].DomainTypes); //Domain of P31 or just Q5?
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes); //Domain of P31 or just Q5?

            Assert.NotEmpty(queryGraph.Edges[1].DomainTypes); //Domain of P31
            Assert.Empty(queryGraph.Edges[1].RangeTypes);

            Assert.Single(queryGraph.Edges[1].DomainTypes); //Domain of P31
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes); //Domain of P31
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var2 ?prop0 ?var0
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are
        /// TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with range in Human 
        /// </summary>
        [Fact]
        public void TestTypes_3ConnectedNodes_2_N0InstanceOfN1_E1RangeN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2")
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 2, 0)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes); //InstanceOf Q5

            Assert.Single(queryGraph.Nodes[0].ParentTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].ParentTypes);
            Assert.Single(queryGraph.Nodes[0].InferredTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[1].InferredTypes);
            Assert.Empty(queryGraph.Nodes[1].ParentTypes);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Nodes[2].InferredTypes);
            Assert.Empty(queryGraph.Nodes[2].ParentTypes);

            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes); //Range of P31

            Assert.Single(queryGraph.Edges[0].DomainTypes); //Domain of P31 or just Q5?
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes); //Domain of P31 or just Q5?

            Assert.Empty(queryGraph.Edges[1].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[1].RangeTypes); //Range of P31

            Assert.Single(queryGraph.Edges[1].RangeTypes); //Range of P31
            Assert.Contains("Q5", queryGraph.Edges[1].RangeTypes); //Range of P31
        }


        /// <summary>
        /// ?human ?prop0 ?country
        /// ?human P31 HUMAN
        /// ?country P31 COUNTRY
        ///
        /// Expected:
        /// ?human IsInstanceOf HUMAN
        /// ?city IsInstanceOf COUNTRY
        /// ?prop0 Intersect Domain HUMAN Range COUNTRY
        /// </summary>
        [Fact]
        public void TestTypes_4ConnectedNodes_N1InstanceOfN3_N2InstanceOfN4_N1E1N2_E1DomainN1RangeN2_4Nodes3Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?human"),
                    new Node(1, "?country"),
                    new Node(2, "human", new[] {"http://www.wikidata.org/entity/Q5"}),
                    new Node(3, "country", new[] {"http://www.wikidata.org/entity/Q6256"})
                },
                edges = new[] {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?type1", 0, 2, new[] {"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?type2", 1, 3, new[] {"http://www.wikidata.org/prop/direct/P31"})
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            //?human
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.NotEmpty(queryGraph.Nodes[0].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].ParentTypes); //InstanceOf Q5

            Assert.Single(queryGraph.Nodes[0].ParentTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].ParentTypes);
            Assert.Single(queryGraph.Nodes[0].InferredTypes);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            //?country
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.NotEmpty(queryGraph.Nodes[1].InferredTypes);
            Assert.NotEmpty(queryGraph.Nodes[1].ParentTypes); //InstanceOf Q6256

            //HUMAN
            Assert.NotEmpty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Nodes[2].InferredTypes);
            Assert.Empty(queryGraph.Nodes[2].ParentTypes);

            Assert.Single(queryGraph.Nodes[2].Types);
            Assert.Contains("Q5", queryGraph.Nodes[2].Types);

            //COUNTRY
            Assert.NotEmpty(queryGraph.Nodes[3].Types);
            Assert.Empty(queryGraph.Nodes[3].InferredTypes);
            Assert.Empty(queryGraph.Nodes[3].ParentTypes);

            Assert.Single(queryGraph.Nodes[3].Types);
            Assert.Contains("Q6256", queryGraph.Nodes[3].Types);

            //?human -> ?country
            Assert.NotEmpty(queryGraph.Edges[0].DomainTypes); //From the Domain of Q5
            Assert.NotEmpty(queryGraph.Edges[0].RangeTypes); //From the Domain of Q6256

            Assert.Single(queryGraph.Edges[0].DomainTypes); //From the Domain of Q5
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes); //From the Domain of Q5
            Assert.Single(queryGraph.Edges[0].RangeTypes); //From the Domain of Q6256
            Assert.Contains("Q6256", queryGraph.Edges[0].RangeTypes); //From the Domain of Q6256

            //?human P31 HUMAN
            Assert.NotEmpty(queryGraph.Edges[1].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[1].RangeTypes); //Range: Q5

            Assert.Single(queryGraph.Edges[1].DomainTypes); //?human is type Q5. Domain should be Q5.
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes); //Domain of P31

            //?country P31 COUNTRY
            Assert.NotEmpty(queryGraph.Edges[2].DomainTypes); //Domain of P31
            Assert.NotEmpty(queryGraph.Edges[2].RangeTypes); //Range: Q6256

            Assert.Single(queryGraph.Edges[2].DomainTypes); //?country is type Q6256. Domain should be Q6256.
            Assert.Contains("Q6256", queryGraph.Edges[2].DomainTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop ?son
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_2ConnectedNodes_N0P25N1_N0DRP25N1_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?human"),
                    new Node(1, "?country")
                },
                edges = new[] {
                    new Edge(0, "?countryOfCitizenship", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(1, "?propDomainRangeP27", 0, 1)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.Single(queryGraph.Nodes[1].InferredTypes);
            Assert.Contains("Q6256", queryGraph.Nodes[1].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Single(queryGraph.Edges[0].RangeTypes);
            Assert.Contains("Q6256", queryGraph.Edges[0].RangeTypes);

            Assert.Equal(2, queryGraph.Edges[1].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes);
            Assert.Single(queryGraph.Edges[1].RangeTypes);
            Assert.Contains("Q6256", queryGraph.Edges[1].RangeTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?son ?prop ?mother
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_2ConnectedNodes_N0P25N1_N1DRP25N0_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?mother"),
                    new Node(1, "?son")
                },
                edges = new[] {
                    new Edge(0, "?motherOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomainRangeP25", 1, 0)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.Equal(5, queryGraph.Nodes[1].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Equal(5, queryGraph.Edges[0].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].RangeTypes);

            Assert.Equal(5, queryGraph.Edges[1].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes);
            Assert.Equal(2, queryGraph.Edges[1].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].RangeTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop ?var2
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1DomainP25_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var2")
                },
                edges = new[] {
                    new Edge(0, "?motherOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomain", 0, 2)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.Equal(5, queryGraph.Nodes[1].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Empty(queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Equal(5, queryGraph.Edges[0].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].RangeTypes);

            //Since E1 source is HUMAN, Domain HUMAN
            Assert.Equal(2, queryGraph.Edges[1].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes);
            Assert.Empty(queryGraph.Edges[1].RangeTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop1 ?var1
        /// ?son ?prop2 ?var1
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1DomainP25_E2DomainP25_3Nodes3Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var1")
                },
                edges = new[] {
                    new Edge(0, "?motherOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?prop1DomainP25", 0, 2),
                    new Edge(2, "?prop2RangeP25", 1, 2)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.Equal(5, queryGraph.Nodes[1].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Empty(queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Equal(5, queryGraph.Edges[0].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].RangeTypes);

            Assert.Equal(2, queryGraph.Edges[1].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes);
            Assert.Empty(queryGraph.Edges[1].RangeTypes);

            Assert.Equal(5, queryGraph.Edges[2].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[2].DomainTypes);
            Assert.Empty(queryGraph.Edges[2].RangeTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?var2 ?prop ?mother
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1RangeP25_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var2")
                },
                edges = new[] {
                    new Edge(0, "?motherOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propRangeP25", 2, 0)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            Assert.Equal(5, queryGraph.Nodes[1].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Empty(queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Equal(5, queryGraph.Edges[0].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].RangeTypes);

            //Since E1 source is HUMAN, Domain HUMAN
            Assert.Empty(queryGraph.Edges[1].DomainTypes);
            Assert.Equal(2, queryGraph.Edges[1].RangeTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].RangeTypes);
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?son P27 ?country
        /// ?mother ?prop ?country
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_N1P27N2_E1DomainP25RangeP27_3Nodes3Edges()
        {
            var graph = new RDFExplorerGraph {
                nodes = new[] {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?country")
                },
                edges = new[] {
                    new Edge(0, "?motherOf", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?fromCountry", 1, 2, new[] {"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(2, "?propDomainRange", 0, 2)
                }
            };

            // Arrange
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].InferredTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);

            //Intersect
            Assert.Single(queryGraph.Nodes[1].InferredTypes);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Single(queryGraph.Nodes[2].InferredTypes);
            Assert.Contains("Q6256", queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(2, queryGraph.Edges[0].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].DomainTypes);
            Assert.Single(queryGraph.Edges[0].RangeTypes);
            Assert.Contains("Q5", queryGraph.Edges[0].RangeTypes);

            Assert.Single(queryGraph.Edges[1].DomainTypes);
            Assert.Contains("Q5", queryGraph.Edges[1].DomainTypes);
            Assert.Single(queryGraph.Edges[1].RangeTypes);
            Assert.Contains("Q6256", queryGraph.Edges[1].RangeTypes);

            Assert.Equal(2, queryGraph.Edges[2].DomainTypes.Count);
            Assert.Contains("Q5", queryGraph.Edges[2].DomainTypes);
            Assert.Single(queryGraph.Edges[2].RangeTypes);
            Assert.Contains("Q6256", queryGraph.Edges[2].RangeTypes);
        }
    }
}