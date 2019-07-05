using SparqlForHumans.Models;
using SparqlForHumans.Models.Extensions;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class ModelsExtensionsTests
    {
        [Fact]
        public void TestGetEntityUri()
        {
            var entity = new Entity
            {
                Id = "Q25"
            };
            Assert.Equal("http://www.wikidata.org/entity/Q25", entity.WikidataUri());
        }

        [Fact]
        public void TestGetPropertyUri()
        {
            var property = new Property
            {
                Id = "P31"
            };
            Assert.Equal("http://www.wikidata.org/prop/direct/P31", property.WikidataUri());
        }

        [Fact]
        public void TestSubjectToString()
        {
            var subject = new Subject
            {
                Id = "Id",
                Label = "Label"
            };
            Assert.Equal("Label (Id)", subject.ToString());
        }
    }
}