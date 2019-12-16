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
        public void TestSparql_2ConnectedNodes_1_SubjectIsGivenType_EdgeP27_2Nodes1Edge()
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
            var expectedStatements = new[]
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
        public void TestSparql_2ConnectedNodes_2_ObjectIsGivenType_EdgeP27_2Nodes1Edge()
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
                    new Edge(0, "?prop0", 0, 1, new []{"http://www.wikidata.org/prop/direct/P27"})
                },
            };
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[0].ToSparql(queryGraph);
            var expectedStatements = new[]
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
        public void TestSparql_2ConnectedNodes_3_SubjectIsGivenType_EdgeVar_2Nodes1Edge()
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
            var expectedStatements = new[]
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
        public void TestSparql_2ConnectedNodes_4_ObjectIsGivenType_EdgeVar_2Nodes1Edge()
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
            var expectedStatements = new[]
            {
                @"?var0 ?prop0 <http://www.wikidata.org/entity/Q76> .",
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }

        [Fact]
        public void TestSparql_5ConnectedNodes_SiblingBrothersDirectorsOfMovies_5Nodes7Edge()
        {
            var graph = new RDFExplorerGraph()
            {
                nodes = new[]
                {
                    new Node(0, "?human0"),
                    new Node(1, "?human1"),
                    new Node(2, "?film2"),
                    new Node(3, "?HUMAN", new[]{"http://www.wikidata.org/entity/Q5"} ),
                    new Node(4, "?FILM", new[]{"http://www.wikidata.org/entity/Q11424"} ),
                },
                edges = new[]
                {
                    new Edge(0, "?instanceOfHuman0", 0, 3, new []{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(1, "?instanceOfHuman1", 1, 3, new []{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(2, "?instanceOfMovie2", 2, 4, new []{"http://www.wikidata.org/prop/direct/P31"}),
                    new Edge(3, "?sibling0", 0, 1, new []{"http://www.wikidata.org/prop/direct/P3373"}),
                    new Edge(4, "?sibling1", 1, 0, new []{"http://www.wikidata.org/prop/direct/P3373"}),
                    new Edge(5, "?director0", 2, 0, new []{"http://www.wikidata.org/prop/direct/P57"}),
                    new Edge(6, "?director1", 2, 1, new []{"http://www.wikidata.org/prop/direct/P57"}),
                },
            };
            var queryGraph = new QueryGraph(graph);
            var sparql = queryGraph.Nodes[0].ToSparql(queryGraph);
            var expectedStatements = new[]
            {
                @"?human0 <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                @"?human1 <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q5> .",
                @"?film2 <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q11424> .",
                @"?human0 <http://www.wikidata.org/prop/direct/P3373> ?human1 .",
                @"?human1 <http://www.wikidata.org/prop/direct/P3373> ?human0 .",
                @"?film2 <http://www.wikidata.org/prop/direct/P57> ?human0 .", 
                @"?film2 <http://www.wikidata.org/prop/direct/P57> ?human1 .", 
            };

            foreach (var statement in expectedStatements)
            {
                Assert.Contains(statement, sparql.ToString());
            }
        }
    }
}
