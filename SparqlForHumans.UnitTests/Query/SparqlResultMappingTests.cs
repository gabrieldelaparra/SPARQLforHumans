using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Wikidata.Services;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    [Collection("Sequential")]
    public class SparqlResultMappingTests
    {
        [Fact]
        public void TestResultToEntity()
        {
            const string query = @"
SELECT DISTINCT ?item
WHERE {
  ?item wdt:P31 wd:Q146.
}
LIMIT 10";

            var results = GraphApiQueries.RunQuery(query).GetIds();

            foreach (var result in results)
            {
                Assert.StartsWith("Q", result);
            }
        }

        [Fact]
        public void TestResultObamaToEntity()
        {
            const string query = @"
SELECT DISTINCT ?item
WHERE {
  wd:Q76 ?prop0 ?item .
  ?item rdfs:label ?o .
}
LIMIT 10";

            var results = GraphApiQueries.RunQuery(query).GetIds();

            foreach (var result in results)
            {
                Assert.StartsWith("Q", result);
            }
        }

        [Fact]
        public void TestFromGraph()
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
            var query = queryGraph.Nodes[1].ToSparql(queryGraph);

            var results = GraphApiQueries.RunQuery(query.ToString()).GetIds();

            foreach (var result in results)
            {
                Assert.True(result.StartsWith("Q") || result.StartsWith("P"));
            }
        }
    }
}
