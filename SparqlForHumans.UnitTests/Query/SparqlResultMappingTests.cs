using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Wikidata.Services;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
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

        //Disable. It crashes since the index is not there. Used for debug
        //[Fact]
        //public void TestFromGraph_DEBUG_VarToHuman_TimesOut()
        //{
        //    var graph = new RDFExplorerGraph()
        //    {
        //        nodes = new[]
        //        {
        //            new Node(0, "?var0"),
        //            new Node(1, "?var1", new[] { "http://www.wikidata.org/entity/Q5"}),
        //        },
        //        edges = new[]
        //        {
        //            new Edge(0, "?prop0", 0, 1)
        //        },
        //    };
        //    var queryGraph = new QueryGraph(graph);

        //    GraphApiQueries.QueryTimeoutMs = 2000;
        //    var queryGraphResults = new QueryGraphResults();
        //    var tasks = queryGraphResults.RunWikidataEndpointQueries(queryGraph);
        //    queryGraphResults.AssignEndpointResults(queryGraph, tasks);

        //    Assert.Empty(queryGraph.Edges[0].Results);
        //}

        //[Fact]
        //public void TestFromGraph_DEBUG_VarEducatedAtCat_EmptyResults()
        //{
        //    var graph = new RDFExplorerGraph()
        //    {
        //        nodes = new[]
        //        {
        //            new Node(0, "?var0"),
        //            new Node(1, "?var1", new[] { "http://www.wikidata.org/entity/Q146"}),
        //        },
        //        edges = new[]
        //        {
        //            new Edge(0, "?prop0", 0, 1, new[] { "http://www.wikidata.org/prop/direct/P69"})
        //        },
        //    };
        //    var queryGraph = new QueryGraph(graph);

        //    GraphApiQueries.QueryTimeoutMs = 2000;
        //    var queryGraphResults = new QueryGraphResults();
        //    var tasks = queryGraphResults.RunWikidataEndpointQueries(queryGraph);
        //    queryGraphResults.AssignEndpointResults(queryGraph, tasks);

        //    Assert.Empty(queryGraph.Edges[0].Results);
        //}


        //[Fact]
        //public void TestFromGraph_DEBUG_VarToCat_NonEmptyResults()
        //{
        //    var graph = new RDFExplorerGraph()
        //    {
        //        nodes = new[]
        //        {
        //            new Node(0, "?var0"),
        //            new Node(1, "?var1", new[] { "http://www.wikidata.org/entity/Q146"}),
        //        },
        //        edges = new[]
        //        {
        //            new Edge(0, "?prop0", 0, 1)
        //        },
        //    };
        //    var queryGraph = new QueryGraph(graph);

        //    //GraphApiQueries.QueryTimeoutMs = 10000;
        //    var queryGraphResults = new QueryGraphResults();
        //    var tasks = queryGraphResults.RunWikidataEndpointQueries(queryGraph);
        //    queryGraphResults.AssignEndpointResults(queryGraph, tasks);

        //    Assert.NotEmpty(queryGraph.Edges[0].Results);
        //}
    }
}
