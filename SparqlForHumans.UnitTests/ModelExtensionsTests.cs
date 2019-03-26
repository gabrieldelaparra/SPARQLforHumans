using SparqlForHumans.Models.Extensions;
using SparqlForHumans.Models.RDFQuery;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class ModelsExtensionsTests
    {
        [Fact]
        public void TestGetEntityUri()
        {
            var entity = new RDFEntity
            {
                Id = "Q25"
            };
            Assert.Equal("http://www.wikidata.org/entity/Q25", entity.WikidataUri());
        }

        [Fact]
        public void TestGetPropertyUri()
        {
            var property = new RDFProperty
            {
                Id = "P31"
            };
            Assert.Equal("http://www.wikidata.org/prop/direct/P31", property.WikidataUri());
        }
    }
}