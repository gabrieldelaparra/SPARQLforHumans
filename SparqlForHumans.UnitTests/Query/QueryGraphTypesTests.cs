using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.", Justification = "<Pending>")]
    public class QueryGraphTypesTests
    {
        private const string EntitiesIndexPath = "QueryGraphTypesEntities";
        private const string PropertiesIndexPath = "QueryGraphTypesProperties";

        private static void CreateIndex()
        {
            // Arrange
            const string filename = @"Resources/QueryGraphTypes.nt";
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, EntitiesIndexPath).Index();
            new SimplePropertiesIndexer(filename, PropertiesIndexPath).Index();
        }

        private static void DeleteIndex()
        {
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
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
        public void TestTypes_2ConnectedNodes_1_SourceIsGivenType_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"} ),
                    new Node(1, "?var1"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].GivenTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[0].GivenTypes.Count);
            Assert.Contains("Q76", queryGraph.Nodes[0].GivenTypes);

            Assert.Empty(queryGraph.Nodes[1].GivenTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfDerivedTypes);

            Assert.Empty(queryGraph.Edges[0].DomainBaseTypes);
            Assert.Empty(queryGraph.Edges[0].RangeBaseTypes);
            Assert.NotEmpty(queryGraph.Edges[0].DomainDerivedTypes);
            Assert.Empty(queryGraph.Edges[0].RangeDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].DomainDerivedTypes.Count);
            Assert.Contains("Q76", queryGraph.Edges[0].DomainDerivedTypes);

            //Cleanup
            DeleteIndex();
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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0" ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q76"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].GivenTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfDerivedTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].GivenTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[1].GivenTypes.Count);
            Assert.Contains("Q76", queryGraph.Nodes[1].GivenTypes);

            Assert.Empty(queryGraph.Edges[0].DomainBaseTypes);
            Assert.Empty(queryGraph.Edges[0].RangeBaseTypes);
            Assert.Empty(queryGraph.Edges[0].DomainDerivedTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].RangeDerivedTypes.Count);
            Assert.Contains("Q76", queryGraph.Edges[0].RangeDerivedTypes);

            //Cleanup
            DeleteIndex();
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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"} ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q30"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].GivenTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[0].GivenTypes.Count);
            Assert.Contains("Q76", queryGraph.Nodes[0].GivenTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].GivenTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[1].GivenTypes.Count);
            Assert.Contains("Q30", queryGraph.Nodes[1].GivenTypes);

            Assert.Empty(queryGraph.Edges[0].DomainBaseTypes);
            Assert.Empty(queryGraph.Edges[0].RangeBaseTypes);
            Assert.NotEmpty(queryGraph.Edges[0].DomainDerivedTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].DomainDerivedTypes.Count);
            Assert.Contains("Q76", queryGraph.Edges[0].DomainDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].RangeDerivedTypes.Count);
            Assert.Contains("Q30", queryGraph.Edges[0].RangeDerivedTypes);

            //Cleanup
            DeleteIndex();
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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"} ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q30"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"})
                },
            };
            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.NotEmpty(queryGraph.Nodes[0].GivenTypes); //Q76
            Assert.NotEmpty(queryGraph.Nodes[0].InferredBasedTypes); //Domain P27
            Assert.Empty(queryGraph.Nodes[0].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[0].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[0].GivenTypes.Count);
            Assert.Contains("Q76", queryGraph.Nodes[0].GivenTypes);
            Assert.Equal(2, queryGraph.Nodes[0].InferredBasedTypes.Count);

            Assert.NotEmpty(queryGraph.Nodes[1].GivenTypes); //Q30
            Assert.NotEmpty(queryGraph.Nodes[1].InferredBasedTypes); //Range P27
            Assert.Empty(queryGraph.Nodes[1].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[1].GivenTypes.Count);
            Assert.Contains("Q30", queryGraph.Nodes[1].GivenTypes);
            Assert.Equal(1, queryGraph.Nodes[1].InferredBasedTypes.Count);

            Assert.NotEmpty(queryGraph.Edges[0].DomainBaseTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeBaseTypes);
            Assert.NotEmpty(queryGraph.Edges[0].DomainDerivedTypes);
            Assert.NotEmpty(queryGraph.Edges[0].RangeDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].DomainDerivedTypes.Count);
            Assert.Contains("Q76", queryGraph.Edges[0].DomainDerivedTypes);

            Assert.Equal(1, queryGraph.Edges[0].RangeDerivedTypes.Count);
            Assert.Contains("Q30", queryGraph.Edges[0].RangeDerivedTypes);

            //Cleanup
            DeleteIndex();
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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0" ),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1,new []{"http://www.wikidata.org/prop/direct/P31"})
                },
            };
            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Empty(queryGraph.Nodes[0].GivenTypes);
            Assert.Empty(queryGraph.Nodes[0].InferredBasedTypes); 
            Assert.Empty(queryGraph.Nodes[0].InferredDerivedTypes);
            Assert.NotEmpty(queryGraph.Nodes[0].InstanceOfBaseTypes); //InstanceOf Q5
            Assert.Empty(queryGraph.Nodes[0].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[0].InstanceOfBaseTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].InstanceOfBaseTypes);

            Assert.NotEmpty(queryGraph.Nodes[1].GivenTypes); 
            Assert.Empty(queryGraph.Nodes[1].InferredBasedTypes);
            Assert.Empty(queryGraph.Nodes[1].InferredDerivedTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfBaseTypes);
            Assert.Empty(queryGraph.Nodes[1].InstanceOfDerivedTypes);

            Assert.Equal(1, queryGraph.Nodes[1].GivenTypes.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].GivenTypes);

            Assert.NotEmpty(queryGraph.Edges[0].DomainBaseTypes); //Domain of P31
            Assert.Empty(queryGraph.Edges[0].RangeBaseTypes); //Range of P31

            Assert.Equal(7, queryGraph.Edges[0].DomainBaseTypes.Count);//Domain of P31
            Assert.Contains("Q5", queryGraph.Edges[0].DomainBaseTypes);//Domain of P31

            //TODO: Creo que los derived types vendran despues.
            Assert.NotEmpty(queryGraph.Edges[0].DomainDerivedTypes);
            Assert.Empty(queryGraph.Edges[0].RangeDerivedTypes);

            Assert.Equal(9, queryGraph.Edges[0].DomainDerivedTypes.Count);
            Assert.Contains("Q76", queryGraph.Edges[0].DomainDerivedTypes);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var0 ?prop0 ?var2
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are //TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with domain in Human 
        /// </summary>
        [Fact]
        public void TestTypes_3ConnectedNodes_1_N0InstanceOfN1_E1DomainN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 0, 2),
                }
            };

            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Single(queryGraph.Edges[0].Range);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            //Since E1 source is HUMAN, Domain HUMAN
            Assert.Single( queryGraph.Edges[1].Domain);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// ?var2 ?prop0 ?var0
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// ?var2 are //TODO: TBD. For the moment, TopEntities;
        /// ?prop1 are properties with range in Human 
        /// </summary>
        [Fact]
        public void TestTypes_3ConnectedNodes_2_N0InstanceOfN1_E1RangeN0_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?prop1", 2, 0),
                }
            };


            // Arrange
            CreateIndex();
            var queryGraph = new QueryGraph(graph);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(8, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Single(queryGraph.Edges[0].Range);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            //Since E1 source is HUMAN, the properties available, should be properties arriving at HUMAN (P25,..)
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Single(queryGraph.Edges[1].Range);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
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
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?human"),
                    new Node(1, "?country"),
                    new Node(2, "human", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(3, "country", new[]{"http://www.wikidata.org/entity/Q6256"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?type1", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?type2", 1, 3, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Single(queryGraph.Nodes[0].Types);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q6256", queryGraph.Nodes[1].Types);

            Assert.Single(queryGraph.Nodes[2].Types);
            Assert.Contains("Q5", queryGraph.Nodes[2].Types);

            Assert.Single(queryGraph.Nodes[3].Types);
            Assert.Contains("Q6256", queryGraph.Nodes[3].Types);

            //From InstanceOf HUMAN
            Assert.Single(queryGraph.Edges[0].Domain);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            //To InstanceOf COUNTRY
            Assert.Single(queryGraph.Edges[0].Range);
            Assert.Contains("Q6256", queryGraph.Edges[0].Range);

            //InstanceOf Edge. Domain contains ?human (Obama). Range contains HUMAN.
            //Assert.Equal(8, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Single(queryGraph.Edges[1].Range);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            //InstanceOf Edge. Domain contains ?city. Range contains COUNTRY.
            Assert.Equal(8, queryGraph.Edges[2].Domain.Count);
            Assert.Contains("Q6256", queryGraph.Edges[2].Domain);
            Assert.Single(queryGraph.Edges[2].Range);
            Assert.Contains("Q6256", queryGraph.Edges[2].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop ?son
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_2ConnectedNodes_N0P25N1_N0DRP25N1_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomainRangeP25", 0, 1),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Equal(6, queryGraph.Nodes[1].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            Assert.Equal(2, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Equal(6, queryGraph.Edges[1].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?son ?prop ?mother
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_2ConnectedNodes_N0P25N1_N1DRP25N0_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomainRangeP25", 1, 0),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Equal(6, queryGraph.Nodes[1].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            Assert.Equal(6, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Equal(2, queryGraph.Edges[1].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop ?var2
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1DomainP25_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomain", 0, 2),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Equal(6, queryGraph.Nodes[1].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            //Since E1 source is HUMAN, Domain HUMAN
            Assert.Equal(2, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?var2 ?prop ?mother
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1RangeP25_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propRangeP25", 2, 0),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Equal(6, queryGraph.Nodes[1].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            //Since E1 source is HUMAN, Domain HUMAN
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Equal(2, queryGraph.Edges[1].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?mother ?prop1 ?var1
        /// ?son ?prop2 ?var1
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_E1DomainP25_E2DomainP25_3Nodes3Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?var1"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?prop1DomainP25", 0, 2),
                    new Edge(2, "?prop2RangeP25", 1, 2),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);
            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);
            Assert.Empty(queryGraph.Edges[2].Domain);
            Assert.Empty(queryGraph.Edges[2].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            Assert.Equal(6, queryGraph.Nodes[1].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            Assert.Equal(2, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);

            Assert.Equal(6, queryGraph.Edges[2].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[2].Domain);
            Assert.Empty(queryGraph.Edges[2].Range);

            //Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?mother P25 ?son
        /// ?son P27 ?country
        /// ?mother ?prop ?country
        /// </summary>
        [Fact]
        public void TestTypes_Inferred_3ConnectedNodes_N0P25N1_N1P27N2_E1DomainP25RangeP27_3Nodes3Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?mother"),
                    new Node(1, "?son"),
                    new Node(2, "?country"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?fromCountry", 1, 2, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(2, "?propDomainRange", 0, 2),
                }
            };

            // Arrange
            CreateIndex();

            var queryGraph = new QueryGraph(graph);
            Assert.Empty(queryGraph.Nodes[0].Types);
            Assert.Empty(queryGraph.Nodes[1].Types);
            Assert.Empty(queryGraph.Nodes[2].Types);

            Assert.Empty(queryGraph.Edges[0].Domain);
            Assert.Empty(queryGraph.Edges[0].Range);
            Assert.Empty(queryGraph.Edges[1].Domain);
            Assert.Empty(queryGraph.Edges[1].Range);
            Assert.Empty(queryGraph.Edges[2].Domain);
            Assert.Empty(queryGraph.Edges[2].Range);

            //Act
            queryGraph.SetIndexPaths(EntitiesIndexPath, PropertiesIndexPath);
            queryGraph.SetTypesDomainsAndRanges();

            //Assert
            Assert.Equal(2, queryGraph.Nodes[0].Types.Count);
            Assert.Contains("Q5", queryGraph.Nodes[0].Types);

            //Intersect
            Assert.Single(queryGraph.Nodes[1].Types);
            Assert.Contains("Q5", queryGraph.Nodes[1].Types);

            Assert.Single(queryGraph.Nodes[2].Types);
            Assert.Contains("Q6256", queryGraph.Nodes[2].Types);

            Assert.Equal(2, queryGraph.Edges[0].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Domain);
            Assert.Equal(6, queryGraph.Edges[0].Range.Count);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);

            Assert.Equal(2, queryGraph.Edges[1].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Single(queryGraph.Edges[1].Range);
            Assert.Contains("Q6256", queryGraph.Edges[1].Range);

            Assert.Equal(2, queryGraph.Edges[2].Domain.Count);
            Assert.Contains("Q5", queryGraph.Edges[2].Domain);
            Assert.Single(queryGraph.Edges[2].Range);
            Assert.Contains("Q6256", queryGraph.Edges[2].Range);

            //Cleanup
            DeleteIndex();
        }
    }
}
