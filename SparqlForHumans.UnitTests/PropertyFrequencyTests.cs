using System.IO;
using System.Linq;
using SparqlForHumans.Lucene.Services;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyFrequencyTests
    {
        [Fact]
        public void TestGetFileFrequency()
        {
            var filename = @"Resources/PropertyFrequencies.nt";
            Assert.True(File.Exists(filename));

            var dictionary = PropertiesFrequency.GetPropertiesFrequency(filename);

            Assert.NotNull(dictionary);

            Assert.Equal(7, dictionary.Count);

            Assert.Equal("P17", dictionary.ElementAt(0).Key);
            Assert.Equal(3, dictionary.ElementAt(0).Value);

            Assert.Equal("P47", dictionary.ElementAt(1).Key);
            Assert.Equal(5, dictionary.ElementAt(1).Value);

            Assert.Equal("P30", dictionary.ElementAt(2).Key);
            Assert.Equal(3, dictionary.ElementAt(2).Value);
        }

        [Fact]
        public void TestGetFileFrequencyWithGarbage()
        {
            var filename = @"Resources/PropertyFrequenciesWithGarbage.nt";
            Assert.True(File.Exists(filename));

            var dictionary = PropertiesFrequency.GetPropertiesFrequency(filename);

            Assert.NotNull(dictionary);

            Assert.Equal(7, dictionary.Count);

            Assert.Equal("P17", dictionary.ElementAt(0).Key);
            Assert.Equal(3, dictionary.ElementAt(0).Value);

            Assert.Equal("P47", dictionary.ElementAt(1).Key);
            Assert.Equal(5, dictionary.ElementAt(1).Value);

            Assert.Equal("P30", dictionary.ElementAt(2).Key);
            Assert.Equal(3, dictionary.ElementAt(2).Value);
        }
    }
}