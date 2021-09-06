using SparqlForHumans.Wikidata.Services;
using Xunit;

namespace SparqlForHumans.UnitTests.Wikidata
{
    public class WikidataQueriesTest
    {
        [Fact]
        public void TestSparqlEndpointWorks_VarToHuman_TimesOut()
        {
            var query = @"
SELECT DISTINCT * WHERE 
{ 
  ?var2 ?prop1 <http://www.wikidata.org/entity/Q5> . 
  FILTER(<http://www.w3.org/2005/xpath-functions#starts-with>(STR(?var2),""http://www.wikidata.org/entity/Q"")) 
  FILTER(<http://www.w3.org/2005/xpath-functions#starts-with>(STR(?prop1),""http://www.wikidata.org/prop/direct/P"")) 
}
LIMIT 10000 
";
            GraphApiQueries.QueryTimeoutMs = 2000;
            var results = GraphApiQueries.RunQuery(query);
            Assert.Null(results);
        }

        [Fact]
        public void TestSparqlEndpointWorks_VarToCat_WorksFine()
        {
            var query = @"
SELECT DISTINCT * WHERE 
{ 
  ?var2 ?prop1 <http://www.wikidata.org/entity/Q146> . 
  FILTER(<http://www.w3.org/2005/xpath-functions#starts-with>(STR(?var2),""http://www.wikidata.org/entity/Q"")) 
  FILTER(<http://www.w3.org/2005/xpath-functions#starts-with>(STR(?prop1),""http://www.wikidata.org/prop/direct/P"")) 
}
LIMIT 10000 
";
            var results = GraphApiQueries.RunQuery(query);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void TestSparqlEndpointWorks_Cats()
        {
            var query = @"
SELECT ?item ?itemLabel 
WHERE 
{
  ?item <http://www.wikidata.org/prop/direct/P31>  <http://www.wikidata.org/entity/Q146>.
  SERVICE wikibase:label { bd:serviceParam wikibase:language ""[AUTO_LANGUAGE],en"". }
}";
            var results = GraphApiQueries.RunQuery(query);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void TestSparqlEndpointWorks_Own()
        {
            var query = @"
SELECT DISTINCT ?var1 ?var1Label
WHERE{  
    <http://www.wikidata.org/entity/Q76> ?prop0 ?var1 . 
    SERVICE <http://wikiba.se/ontology#label> { <http://www.bigdata.com/rdf#serviceParam> <http://wikiba.se/ontology#language> ""[AUTO_LANGUAGE],en"" .}
}";
            var results = GraphApiQueries.RunQuery(query);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void TestSparqlEndpointWorksAsync_Cats()
        {
            var query = @"
SELECT ?item ?itemLabel 
WHERE 
{
  ?item <http://www.wikidata.org/prop/direct/P31>  <http://www.wikidata.org/entity/Q146>.
  SERVICE wikibase:label { bd:serviceParam wikibase:language ""[AUTO_LANGUAGE],en"". }
}";
            var resultTask = GraphApiQueries.RunQueryAsync(query);
            resultTask.Wait();
            var results = resultTask.Result;
            Assert.NotEmpty(results);
        }
    }
}
