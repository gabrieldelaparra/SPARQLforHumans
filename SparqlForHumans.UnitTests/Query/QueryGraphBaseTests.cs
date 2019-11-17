using System.Linq;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphBaseTests
    {
        [Fact]
        public void TestRunQueryScenario1_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node
                    {
                        id = 0,
                        name = "?var0"
                    },
                    new Node
                    {
                        id = 1,
                        name = "?var1"
                    }
                }
            };

            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRunQueryScenario1_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node
                    {
                        id = 0,
                        name = "?var0"
                    },
                    new Node
                    {
                        id = 1,
                        name = "?var1"
                    }
                },
                edges = new[]
                {
                    new Edge
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1
                    }
                }
            };

            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P31"));
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Label.Equals("Instance Of"));

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRunQueryScenario2_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                    new Node(2, "?var2", new[]{"http://www.wikidata.org/entity/Q5"}),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1),
                    new Edge(1, "?prop1", 0, 2,  new[]{"http://www.wikidata.org/prop/direct/P31"})
                }
            };

            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            Assert.Empty(queryGraph.Nodes[2].Results);

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P27"));

            Assert.Empty(queryGraph.Edges[1].Results);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRunQueryScenario3_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1"),
                    new Node(2, "?var2", new[]{"http://www.wikidata.org/entity/Q5"})
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 1, 0),
                    new Edge(1, "?prop1", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P31"})
                }
            };

            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.Empty(queryGraph.Nodes[1].Results);

            Assert.Empty(queryGraph.Nodes[2].Results);

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P25"));

            Assert.Empty(queryGraph.Edges[1].Results);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRunQueryScenario4_4Nodes3Edge()
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

            //Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q298"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Chile"));

            Assert.Empty(queryGraph.Nodes[2].Results);
            Assert.Empty(queryGraph.Nodes[3].Results);

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P27"));

            Assert.Empty(queryGraph.Edges[1].Results);
            Assert.Empty(queryGraph.Edges[2].Results);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }


        [Fact]
        public void TestInferScenario7_2Nodes2Edges_FullIndex()
        {
            // Arrange
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node
                    {
                        id = 0,
                        name = "?varDomain0"
                    },
                    new Node
                    {
                        id = 1,
                        name = "?varRange1"
                    }
                },
                edges = new[]
                {
                    new Edge
                    {
                        id = 0,
                        name = "?CountryOfCitizenship",
                        sourceId = 0,
                        targetId = 1,
                        uris = new[]{"http://www.wikidata.org/prop/direct/P27"}
                    },
                    new Edge
                    {
                        id = 1,
                        name = "?propDomainRange1",
                        sourceId = 0,
                        targetId = 1
                    }

                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);

            // Assert
            //Assert.NotEmpty(queryGraph.Nodes[0].Results);
            //Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            //Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            //Assert.NotEmpty(queryGraph.Nodes[1].Results);
            //queryGraph.Nodes[1].Results = TOP;

            Assert.Empty(queryGraph.Edges[0].Results);
            //Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P25"));

            Assert.NotEmpty(queryGraph.Edges[1].Results);
        }

        [Fact]
        public void TestEqualGraphs()
        {
            // Arrange
            var graph1 = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?varDomain0"),
                    new Node(1, "?varRange1")
                },
                edges = new[]
                {
                    new Edge(0, "?CountryOfCitizenship", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(1, "?propDomainRange1", 0, 1)

                }
            };

            var graph2 = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?varDomain0"),
                    new Node(1, "?varRange1")
                },
                edges = new[]
                {
                    new Edge(0, "?CountryOfCitizenship", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(1, "?propDomainRange1", 0, 1)

                }
            };

            //Are equal:
            Assert.Equal(graph1, graph2);

            //Node: Change name
            graph1.nodes[0].name = "cambio";
            Assert.Equal(graph1, graph2);


            //Node: Change Id
            graph1.nodes[0].id = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.nodes[0].id = 0;
            Assert.Equal(graph1, graph2);

            //Node: Change uris
            graph1.nodes[0].uris = new[] { "uri1" };
            Assert.NotEqual(graph1, graph2);
            graph1.nodes[0].uris = new string[] { };
            Assert.Equal(graph1, graph2);

            //Node: Add node
            graph1.nodes = graph1.nodes.ToList().Append(new Node { id = 2, name = "new" }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.nodes = graph1.nodes.ToList().Take(2).ToArray();
            Assert.Equal(graph1, graph2);

            //Node: Remove Node
            graph1.nodes = graph1.nodes.ToList().Take(1).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.nodes = graph1.nodes.ToList().Append(new Node { id = 1, name = "?varRange1" }).ToArray();
            Assert.Equal(graph1, graph2);

            //Edge: Change name
            graph1.edges[0].name = "cambio";
            Assert.Equal(graph1, graph2);

            //Edge: change Id
            graph1.edges[0].id = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].id = 0;
            Assert.Equal(graph1, graph2);

            //Edge: change source Id
            graph1.edges[0].sourceId = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].sourceId = 0;
            Assert.Equal(graph1, graph2);

            //Edge: change target Id
            graph1.edges[0].targetId = 2;
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].targetId = 1;
            Assert.Equal(graph1, graph2);

            //Edge: Change uris
            graph1.edges[0].uris = new string[] { };
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].uris = new[] { "http://www.wikidata.org/prop/direct/P27", "http://www.wikidata.org/prop/direct/P27" };
            Assert.NotEqual(graph1, graph2);
            graph1.edges[0].uris = new[] { "http://www.wikidata.org/prop/direct/P27" };
            Assert.Equal(graph1, graph2);

            //Edge: Add node
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 2, name = "new" }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Take(2).ToArray();
            Assert.Equal(graph1, graph2);

            //Edge: Remove Node
            graph1.edges = graph1.edges.ToList().Take(1).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 1, name = "?warever", sourceId = 0, targetId = 0 }).ToArray();
            Assert.NotEqual(graph1, graph2);
            graph1.edges = graph1.edges.ToList().Take(1).ToArray();
            graph1.edges = graph1.edges.ToList().Append(new Edge { id = 1, name = "?warever", sourceId = 0, targetId = 1 }).ToArray();
            Assert.Equal(graph1, graph2);

            //Selected
            graph1.selected = new Selected { id = 0, isNode = true };
            Assert.Equal(graph1, graph2);
            graph1.selected = new Selected { id = 1, isNode = false };
            Assert.Equal(graph1, graph2);
        }

        [Fact]
        public void TestPropertiesGoingToObamaShouldBeHumanProperties()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1",new[]{"http://www.wikidata.org/entity/Q76"} )
                },
                edges = new[]
                {
                    new Edge (0, "?prop0", 0, 1)
                }
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenObjectTypeQueryDirectly, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenObjectTypeQueryIncomingProperties, queryGraph.Edges[0].QueryType);
        }

        [Fact]
        public void TestUnknownSubjectKnownPredicateKnownObjectShouldThenNotBeInferred()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?siblingOfObama"),
                    new Node(1, "OBAMA", new[]{"http://www.wikidata.org/entity/Q76"})
                },
                edges = new[]
                {
                    new Edge(0, "sibling", 0, 1,  new[]{"http://www.wikidata.org/prop/direct/P3373"})
                }
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.GivenObjectTypeQueryDirectly, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.GivenEntityTypeNoQuery, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
        }

    }
}
