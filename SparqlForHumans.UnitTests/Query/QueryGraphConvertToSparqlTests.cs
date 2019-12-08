using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryGraphConvertToSparqlTests
    {
        /// <summary>
        /// Q76 P27 ?var1 .
        /// </summary>
        [Fact]
        public void TestSparql2ConnectedNodesSubjectIsGivenType_EdgeP27_2Nodes1Edge()
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
                    new Edge(0, "?prop0", 0, 1,new []{"http://www.wikidata.org/prop/direct/P27"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[1].ToSparql(queryGraph);
            var expectedStatements = new []
            {
                @"<http://www.wikidata.org/entity/Q76> <http://www.wikidata.org/prop/direct/P27> ?var1 .",
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }

        /// <summary>
        /// ?var0 P27 Q76 .
        /// </summary>
        [Fact]
        public void TestSparql2ConnectedNodesObjectIsGivenType_EdgeP27_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q76"} ),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1,new []{"http://www.wikidata.org/prop/direct/P27"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[0].ToSparql(queryGraph);
            var expectedStatements = new []
            {
                @"?var0 <http://www.wikidata.org/prop/direct/P27> <http://www.wikidata.org/entity/Q76> .",
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }

        /// <summary>
        /// Q76 ?prop0 ?var1 .
        /// </summary>
        [Fact]
        public void TestSparql2ConnectedNodesSubjectIsGivenType_EdgeVar_2Nodes1Edge()
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
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[1].ToSparql(queryGraph);
            var expectedStatements = new []
            {
                @"<http://www.wikidata.org/entity/Q76> ?prop0 ?var1 .",
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }

        /// <summary>
        /// ?var0 ?prop0 Q76 .
        /// </summary>
        [Fact]
        public void TestSparql2ConnectedNodesObjectIsGivenType_EdgeVar_2Nodes1Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?var0"),
                    new Node(1, "?var1", new[]{"http://www.wikidata.org/entity/Q76"} ),
                },
                edges = new[]
                {
                    new Edge(0, "?prop0", 0, 1)
                },
            };
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[0].ToSparql(queryGraph);
            var expectedStatements = new []
            {
                @"?var0 ?prop0 <http://www.wikidata.org/entity/Q76> .",
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }
    }
}
