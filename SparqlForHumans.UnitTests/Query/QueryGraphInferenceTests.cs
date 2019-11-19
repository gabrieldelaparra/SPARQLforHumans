using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    //TODO: Comment cases
    public class QueryGraphInferenceTests
    {
        [Fact]
        public void TestInferScenario5_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?range"),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomain", 0, 2),
                }
            };

            var queryGraph = new QueryGraph(graph);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[1].QueryType);
        }

        [Fact]
        public void TestTypesScenario5_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?range"),
                    new Node(2, "?var2"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomain", 0, 2),
                }
            };

            var queryGraph = new QueryGraph(graph);

            // Assert
            
        }

        [Fact]
        public void TestInferScenario5_4Nodes3Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?range"),
                    new Node(2, "?var2"),
                    new Node(3, "?var3"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propDomain1", 0, 2),
                    new Edge(2, "?propDomain2", 0, 3),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[2].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[3].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[1].QueryType);
            Assert.Equal(QueryType.InferredDomainTypeProperties, queryGraph.Edges[2].QueryType);
        }

        [Fact]
        public void TestInferScenario6_3Nodes2Edge()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?var1"),
                    new Node(2, "?range"),
                },
                edges = new[]
                {
                    new Edge(0, "?propRange", 1, 0),
                    new Edge(1, "?motherOf", 0, 2, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.QueryTopEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.InferredRangeTypeProperties, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[1].QueryType);
        }

        [Fact]
        public void TestInferScenario7_2Nodes2Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?range"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?propRange", 0, 1),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[1].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeProperties, queryGraph.Edges[1].QueryType);
        }

        [Fact]
        public void TestInferScenario7_3Nodes3Edges()
        {
            var graph = new RDFExplorerGraph
            {
                nodes = new[]
                {
                    new Node(0, "?domain"),
                    new Node(1, "?range"),
                    new Node(2, "?domainRange"),
                },
                edges = new[]
                {
                    new Edge(0, "?motherOf", 0, 1, new[]{"http://www.wikidata.org/prop/direct/P25"}),
                    new Edge(1, "?fromCountry", 1, 2, new[]{"http://www.wikidata.org/prop/direct/P27"}),
                    new Edge(2, "?propDomainRange", 0, 2),
                }
            };

            // Act
            var queryGraph = new QueryGraph(graph);

            // Assert
            Assert.Equal(QueryType.InferredDomainTypeEntities, queryGraph.Nodes[0].QueryType);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeEntities, queryGraph.Nodes[1].QueryType);
            Assert.Equal(QueryType.InferredRangeTypeEntities, queryGraph.Nodes[2].QueryType);

            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[0].QueryType);
            Assert.Equal(QueryType.GivenPredicateTypeNoQuery, queryGraph.Edges[1].QueryType);
            Assert.Equal(QueryType.InferredDomainAndRangeTypeProperties, queryGraph.Edges[2].QueryType);
        }


    }
}
