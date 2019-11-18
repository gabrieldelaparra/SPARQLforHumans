using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphQueryingTests
    {
        const string EntitiesIndexPath = "QueryGraphEntities";
        const string PropertiesIndexPath = "QueryGraphProperties";

        public static void CreateIndex()
        {
            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, EntitiesIndexPath).Index();
            new PropertiesIndexer(filename, PropertiesIndexPath).Index();
        }

        public static void DeleteIndex()
        {
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
        }

        /// <summary>
        /// A single node.
        /// Should query for the top Entities.
        /// In the given example QueryGraph.nt, Obama should be in the top values.
        /// </summary>
        [Fact]
        public void TestResults1Node_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[] { new Node(0, "?var0") },
            };

            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// A single node. Given Type.
        /// Should return given entity types.
        /// </summary>
        [Fact]
        public void TestResults1NodeGivenType_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[] { new Node(0, "?varObama", new[] { "http://www.wikidata.org/entity/Q76" }) },
            };

            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// Two isolated nodes, no properties between them.
        /// Both should be Top Entities.
        /// </summary>
        [Fact]
        public void TestResults2IsolatedNodes_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                },
            };
            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// Two isolated nodes, no properties between them.
        /// Both should be given entity types.
        /// </summary>
        [Fact]
        public void TestResults2IsolatedNodesGivenTypes_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[]{"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q49089"}),
                },
            };
            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q49089"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Human 1"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// No given Types for anyone.
        /// All should return Top Entities and Properties;
        /// </summary>
        [Fact]
        public void TestResults2ConnectedNodesNoGivenTypes_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };

            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q49089"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Human 1"));

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P25"));
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Label.Equals("Mother Of"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var0 is Obama
        /// Expected Results:
        /// ?var0 is a given type
        /// ?var1 is a type that can be accessed only from Obama.
        /// ?var1 is an Inferred Domain Type.
        /// This looks like it should be a query.
        /// </summary>
        [Fact]
        public void TestResults2ConnectedNodesSourceIsGivenType_2Nodes1Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q49089"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Human 1"));

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P25"));
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Label.Equals("Mother Of"));

            // Cleanup
            DeleteIndex();
        }

        /// <summary>
        /// ?var0 ?prop0 ?var1
        /// ?var1 is Obama
        /// 
        /// Expected Results:
        /// ?var0 are types going to Obama
        /// ?prop0 are properties going to Obama
        /// </summary>
        [Fact]
        public void TestResults2ConnectedNodesTargetIsGivenType_2Nodes1Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.Equal(QueryType.GivenObjectTypeQueryDirectly, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.GivenObjectTypeQueryIncomingProperties, queryGraph.Edges[0].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
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
        public void TestResults2ConnectedNodesSourceAndTargetIsGivenType_2Nodes1Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.NotEmpty(queryGraph.Nodes[0].Results);
            //Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            //Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            DeleteIndex();
            Assert.False(true);
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
        public void TestResults2ConnectedNodesSourceAndTargetIsGivenType_E0N0P27E1_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0", new[] {"http://www.wikidata.org/entity/Q76"}),
                    new Node(1, "?var1", new[] {"http://www.wikidata.org/entity/Q30"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1, new[] {"http://www.wikidata.org/prop/direct/P27"})
                },
            };

            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
        }

        /// <summary>
        /// ?var0 P31 ?var1
        /// ?var1 is Human
        /// 
        /// Expected Results:
        /// ?var0 are instances of Human
        /// </summary>
        [Fact]
        public void TestResults2ConnectedNodesSubjectIsInstanceOfType_2Nodes1Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
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
        public void TestResults3ConnectedNodes_N0InstanceOfN1_E1DomainN0_3Nodes2Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            //Assert.Equal(QueryType.KnownSubjectTypeQueryDomainProperties, queryGraph.Edges[1].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
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
        public void TestResults3ConnectedNodes_N0InstanceOfN1_E1RangeN0_3Nodes2Edge()
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

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);

            // Assert
            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            //Assert.Equal(QueryType.KnownObjectTypeQueryRangeProperties, queryGraph.Edges[1].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
        }

        /// <summary>
        /// ?human ?prop0 ?city
        /// ?human P31 HUMAN
        /// ?city P31 CITY
        ///
        /// Expected:
        /// ?human IsInstanceOf HUMAN
        /// ?city IsInstanceOf CITY
        /// ?prop0 Intersect Domain HUMAN Range CITY
        /// </summary>
        [Fact]
        public void TestResults4ConnectedNodes_N1InstanceOfN3_N2InstanceOfN4_N1E1N2_E1DomainN1RangeN2_4Nodes3Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?human"),
                    new Node(1, "?city"),
                    new Node(2, "human", new[]{"http://www.wikidata.org/entity/Q5"}),
                    new Node(3, "city", new[]{"http://www.wikidata.org/entity/Q515"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0,1),
                    new Edge(1, "?type1", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?type2", 1, 3, new[]{"http://www.wikidata.org/prop/direct/P31"}),
                }
            };

            // Arrange
            CreateIndex();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(EntitiesIndexPath, PropertiesIndexPath);
            
            // Assert
            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[0].QueryType);
            //Assert.Equal(QueryType.SubjectIsInstanceOfTypeQueryEntities, queryGraph.Nodes[1].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[2].QueryType);
            //Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[3].QueryType);

            //Assert.Equal(QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties, queryGraph.Edges[0].QueryType);
            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[1].QueryType);
            //Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[2].QueryType);

            // Cleanup
            DeleteIndex();
            Assert.False(true);
        }
    }
}
