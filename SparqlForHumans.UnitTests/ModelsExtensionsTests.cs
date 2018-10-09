using SparqlForHumans.Models;
using SparqlForHumans.Models.Extensions;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class ModelsExtensionsTests
    {
        [Fact]
        public void TestGetEntityURI()
        {
            var entity = new Entity()
            {
                Id = "Q25",
            };
            Assert.Equal("<http://www.wikidata.org/entity/Q25>", entity.Uri());
        }

        [Fact]
        public void TestGetPropertyURI()
        {
            var property = new Property()
            {
                Id = "P31",
            };
            Assert.Equal("<http://www.wikidata.org/prop/direct/P31>", property.Uri());
        }
    }
}
