using Xunit;

namespace SparqlForHumans.UnitTests.Wikidata
{
    [Collection("Sequential")]
    public class WikidataEndpointTests
    {
        [Fact]
        public void TestWikidataSearchByLabel()
        {
            var query = SparqlForHumans.Wikidata.Services.EndpointApiQueries.QueryWikidataSearchByLabel("Obama");
            Assert.NotNull(query);
            Assert.NotEmpty(query.Results);
            Assert.Equal("Q76", query.Results[0].Id);
        }

        [Fact]
        public void TestWikidataQueryEntityById()
        {
            var query = SparqlForHumans.Wikidata.Services.EndpointApiQueries.QueryWikidataEntitiesById("Q76");
            Assert.NotNull(query);
            Assert.NotEmpty(query.Results);
            Assert.Equal("Q76", query.Results["Q76"].Id);
            Assert.Equal("Barack Obama", query.Results["Q76"].Label.ToString());
        }

        [Fact]
        public void TestWikidataQueryPropertyById()
        {
            var query = SparqlForHumans.Wikidata.Services.EndpointApiQueries.QueryWikidataPropertiesById("P25");
            Assert.NotNull(query);
            Assert.NotEmpty(query.Results);
            Assert.Equal("P25", query.Results["P25"].Id);
            Assert.Equal("mother", query.Results["P25"].Label.ToString());
        }
    }
}
