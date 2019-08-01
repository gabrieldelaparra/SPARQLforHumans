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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
            // I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
            // I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varCity",
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

            // Q1 should be something like: I don't know anything about this type. 
            // I know that I have properties in the graph that come from Q5. This node is in the range of Q5.
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
                    },

                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

            // Assert
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
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                    },
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
                    }
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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
            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();
            
            // Act
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?var0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
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

            // Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

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
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varHuman",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varCity",
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

            //Arrange
            const string filename = @"Resources/QueryGraph.nt";
            const string entitiesIndexPath = "QueryGraphEntities";
            const string propertiesIndexPath = "QueryGraphProperties";
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();

            new EntitiesIndexer(filename, entitiesIndexPath).Index();
            new PropertiesIndexer(filename, propertiesIndexPath).Index();

            // Act
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);
            queryGraph.RunGraphQueryResults();

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
        public void TestInferScenario5_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varDomain",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varRange",
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                    },

                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?MotherOf",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P25"}
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?propDomain1",
                        sourceId = 0,
                        targetId = 2,
                    },
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[1].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);

            // Assert RunQuery
            queryGraph.RunGraphQueryResults();
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            //queryGraph.Nodes[2].Results = TOP;
            Assert.Empty(queryGraph.Edges[0].Results);

            Assert.NotEmpty(queryGraph.Edges[1].Results);
            Assert.Contains(queryGraph.Edges[1].Results, x => x.Id.Equals("P25"));

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestInferScenario5_4Nodes3Edges()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varDomain0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varRange1",
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?var2",
                    },
                    new Node()
                    {
                        id = 3,
                        name = "?var3",
                    },

                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?MotherOf",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P25"}
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?propDomain1",
                        sourceId = 0,
                        targetId = 2,
                    },
                    new Edge()
                    {
                        id = 2,
                        name = "?propDomain1",
                        sourceId = 0,
                        targetId = 3,
                    },
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[3].QueryType);

            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[1].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[2].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[2].Domain);

            // Assert RunQuery
            queryGraph.RunGraphQueryResults();
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Nodes[1].Results);
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[1].Results, x => x.Label.Equals("Barack Obama"));

            //queryGraph.Nodes[2].Results = TOP;
            //queryGraph.Nodes[3].Results = TOP;

            Assert.Empty(queryGraph.Edges[0].Results);

            Assert.NotEmpty(queryGraph.Edges[1].Results);
            Assert.Contains(queryGraph.Edges[1].Results, x => x.Id.Equals("P25"));
            Assert.NotEmpty(queryGraph.Edges[2].Results);
            Assert.Contains(queryGraph.Edges[2].Results, x => x.Id.Equals("P25"));

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestInferScenario6_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varDomain",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?var1",
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?varRange2",
                    },

                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?propRange0",
                        sourceId = 1,
                        targetId = 0,
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?MotherOf",
                        sourceId = 0,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P25"}
                    },
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[2].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(QueryType.InferredRangeTypeProperties, queryGraph.Edges[0].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[0].Range);
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[1].QueryType);

            // Assert RunQuery
            queryGraph.RunGraphQueryResults();
            Assert.NotEmpty(queryGraph.Nodes[0].Results);
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[0].Results, x => x.Label.Equals("Barack Obama"));
           
            //queryGraph.Nodes[1].Results = TOP;

            Assert.NotEmpty(queryGraph.Nodes[2].Results);
            Assert.Contains(queryGraph.Nodes[2].Results, x => x.Id.Equals("Q76"));
            Assert.Contains(queryGraph.Nodes[2].Results, x => x.Label.Equals("Barack Obama"));

            Assert.NotEmpty(queryGraph.Edges[0].Results);
            Assert.Contains(queryGraph.Edges[0].Results, x => x.Id.Equals("P25"));
            
            Assert.Empty(queryGraph.Edges[1].Results);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestInferScenario7_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varDomain0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varRange1",
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?MotherOf",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P25"}
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?propDomainRange1",
                        sourceId = 0,
                        targetId = 1,
                    },
                    
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);

            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeProperties, queryGraph.Edges[1].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[1].Domain);
            Assert.Contains("Q5", queryGraph.Edges[1].Range);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestInferScenario7_3Nodes3Edges()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node()
                    {
                        id = 0,
                        name = "?varDomain0",
                    },
                    new Node()
                    {
                        id = 1,
                        name = "?varDomainRange1",
                    },
                    new Node()
                    {
                        id = 2,
                        name = "?varRange2",
                    },
                },
                edges = new[]
                {
                    new Edge()
                    {
                        id = 0,
                        name = "?MotherOf",
                        sourceId = 0,
                        targetId = 1,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P25"}
                    },
                    new Edge()
                    {
                        id = 1,
                        name = "?FromCountry",
                        sourceId = 1,
                        targetId = 2,
                        uris = new string[]{"http://www.wikidata.org/prop/direct/P27"}
                    },
                    new Edge()
                    {
                        id = 2,
                        name = "?propDomainRange1",
                        sourceId = 0,
                        targetId = 2,
                    },
                    
                },
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
            var queryGraph = new QueryGraph(graph, entitiesIndexPath, propertiesIndexPath);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[0].InferredTypes);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Contains("Q5", queryGraph.Nodes[1].InferredTypes);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[2].QueryType);
            Assert.Contains("Q6256", queryGraph.Nodes[2].InferredTypes);

            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.ConstantTypeDoNotQuery, queryGraph.Edges[1].QueryType);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeProperties, queryGraph.Edges[2].QueryType);
            Assert.Contains("Q5", queryGraph.Edges[2].Domain);
            Assert.Contains("Q6256", queryGraph.Edges[2].Range);

            // Cleanup
            entitiesIndexPath.DeleteIfExists();
            propertiesIndexPath.DeleteIfExists();
        }
    }
}
