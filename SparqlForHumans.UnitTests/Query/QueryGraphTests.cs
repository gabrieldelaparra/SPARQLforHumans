using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Models.Query;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphTests
    {
        [Fact]
        public void TestInferScenario1_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },

                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
        }

        [Fact]
        public void TestInferScenario1_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
        }

        [Fact]
        public void TestInferScenario1_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    }
                },
            };
            var queryGraph = new QueryGraph(graph);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.QueryTopProperties, queryGraph.Edges[0].QueryType);
        }

        [Fact]
        public void TestInferScenario2_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?prop1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };
            var queryGraph = new QueryGraph(graph);

            // Node 0 is type Q5. 
            // Results should be something like: I know the type of this guy, should return items of type Q5 (Use Wikidata)
            Assert.Equal(QueryType.KnownSubjectTypeQueryInstanceEntities, queryGraph.Nodes[0].QueryType);

            // Q1 should be something like: I don't know anything about this type.
            // TODO: But actually, I do know somwthing about this node: I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
            // Not implemented yet.
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Nodes[2].QueryType);

            // Edge source is Known. Results should be Domain of the node type (Use Endpoint)
            Assert.Equal(QueryType.KnownSubjectTypeOnlyQueryDomainProperties, queryGraph.Edges[0].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[1].QueryType);
        }

        [Fact]
        public void TestInferScenario3_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 1,
                        targetId = 0,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?prop1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };
            var queryGraph = new QueryGraph(graph);

            // Node 0 is type Q5. 
            // Results should be something like: I know the type of this guy, should return items of type Q5 (Use Wikidata)
            Assert.Equal(QueryType.KnownSubjectTypeQueryInstanceEntities, queryGraph.Nodes[0].QueryType);

            // Q1 should be something like: I don't know anything about this type.
            // TODO: But actually, I do know somwthing about this node: I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
            // Not implemented yet.
            Assert.Equal(QueryType.KnownObjectTypeNotUsed, queryGraph.Nodes[1].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Nodes[2].QueryType);

            // Edge source is Known. Results should be Domain of the node type (Use Endpoint)
            Assert.Equal(QueryType.KnownObjectTypeOnlyQueryRangeProperties, queryGraph.Edges[0].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[1].QueryType);
        }

        [Fact]
        public void TestInferScenario4_4Nodes3Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varHuman",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varCity",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "human",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                    new Node()
                    {
                        id = 3,
                        name = "city",
                        uris = new string[]{"http://www.wikidata.org/entity/Q515"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?type1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    },
                    new Edge()
                    {
                        id = 2,
                        name = "?type2",
                        sourceId = 1,
                        targetId = 3,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };
            var queryGraph = new QueryGraph(graph);

            // Node 0 is type Q5. 
            // Results should be something like: I know the type of this guy, should return items of type Q5 (Use Wikidata)
            Assert.Equal(QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities, queryGraph.Nodes[0].QueryType);
            //TODO: Take out this case;

            // Q1 should be something like: I don't know anything about this type.
            // TODO: But actually, I do know somwthing about this node: I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
            // Not implemented yet.
            Assert.Equal(QueryType.KnownSubjectTypeQueryInstanceEntities, queryGraph.Nodes[1].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Nodes[2].QueryType);
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Nodes[3].QueryType);

            // Edge source is Known. Results should be Domain of the node type (Use Endpoint)
            Assert.Equal(QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties, queryGraph.Edges[0].QueryType);

            // Constant, should not have results.
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[1].QueryType);
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[2].QueryType);
        }

        [Fact]
        public void TestRunQueryScenario1_1Node0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },

                },
            };
            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestRunQueryScenario1_2Nodes0Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                },
            };
            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    }
                },
            };
            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?prop1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };

            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 1,
                        targetId = 0,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?prop1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };

            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varHuman",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varCity",
                        uris = new string[0]
                    },
                    new Node()
                    {
                        id = 2,
                        name = "human",
                        uris = new string[]{"http://www.wikidata.org/entity/Q5"}
                    },
                    new Node()
                    {
                        id = 3,
                        name = "city",
                        uris = new string[]{"http://www.wikidata.org/entity/Q6256"}
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?prop0",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[0]
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?type1",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    },
                    new Edge()
                    {
                        id = 2,
                        name = "?type2",
                        sourceId = 1,
                        targetId = 3,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P31"}
                    }
                },
            };

            var queryGraph = new QueryGraph(graph);
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            // Act
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            queryGraph.RunGraphQueryResults(entitiesIndexPath, propertiesIndexPath);

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
    }
}
