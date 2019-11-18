using System;
using SparqlForHumans.Wikidata.Models;
using SparqlForHumans.Wikidata.Services;
using VDS.RDF.Query;
using Xunit;

namespace SparqlForHumans.UnitTests.Wikidata
{
    public class WikidataQueriesTest
    {
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
    }
}
