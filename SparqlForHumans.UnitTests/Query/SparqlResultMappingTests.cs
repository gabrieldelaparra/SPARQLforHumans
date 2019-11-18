using System.Linq;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class SparqlResultMappingTests
    {
        [Fact]
        public void TestResultToEntity()
        {
            const string query = @"
SELECT DISTINCT ?item ?itemLabel 
WHERE {
  ?item wdt:P31 wd:Q146.
  SERVICE wikibase:label { bd:serviceParam wikibase:language ""[AUTO_LANGUAGE],en"". }
}
LIMIT 10";

            var results = GraphApiQueries.RunQuery(query);
            var entities = results.Select(x => x.ToEntity()).ToList();

            foreach (var entity in entities)
            {
                Assert.StartsWith("Q", entity.Id);
                Assert.NotEmpty(entity.Label);
            }
        }

        [Fact]
        public void TestResultObamaToEntity()
        {
            const string query = @"
SELECT DISTINCT ?item ?itemLabel 
WHERE {
  wd:Q76 ?prop0 ?item .
  ?item rdfs:label ?o .
  SERVICE wikibase:label { bd:serviceParam wikibase:language ""[AUTO_LANGUAGE],en"". }
}
LIMIT 10";

            var results = GraphApiQueries.RunQuery(query);
            var entities = results.Select(x => x.ToEntity()).ToList();

            foreach (var entity in entities)
            {
                Assert.StartsWith("Q", entity.Id);
                Assert.NotEmpty(entity.Label);
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

            var results = GraphApiQueries.RunQuery(query.ToString());
            var entities = results.Select(x => x.ToEntity()).ToList();

            foreach (var entity in entities)
            {
                Assert.StartsWith("Q", entity.Id);
                Assert.NotEmpty(entity.Label);
            }
        }
    }
}
