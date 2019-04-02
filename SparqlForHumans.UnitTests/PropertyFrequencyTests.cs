using System.IO;
using System.Linq;
using SparqlForHumans.Lucene.Indexing;
using SparqlForHumans.Lucene.Relations;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class PropertyFrequencyTests
    {
        [Fact]
        public void TestGetFileFrequency()
        {
            const string filename = @"Resources/PropertyFrequencies.nt";
            Assert.True(File.Exists(filename));

            var dictionary = new PropertiesFrequencyRelationMapper().GetRelationDictionary(filename);

            Assert.NotNull(dictionary);

            Assert.Equal(7, dictionary.Count);

            Assert.Equal(17, dictionary.ElementAt(0).Key);
            Assert.Equal(3, dictionary.ElementAt(0).Value);

            Assert.Equal(47, dictionary.ElementAt(1).Key);
            Assert.Equal(5, dictionary.ElementAt(1).Value);

            Assert.Equal(30, dictionary.ElementAt(2).Key);
            Assert.Equal(3, dictionary.ElementAt(2).Value);
        }

        [Fact]
        public void TestGetFileFrequencyWithGarbage()
        {
            const string filename = @"Resources/PropertyFrequenciesWithGarbage.nt";
            Assert.True(File.Exists(filename));

            var dictionary = new PropertiesFrequencyRelationMapper().GetRelationDictionary(filename);

            Assert.NotNull(dictionary);

            Assert.Equal(7, dictionary.Count);

            Assert.Equal(17, dictionary.ElementAt(0).Key);
            Assert.Equal(3, dictionary.ElementAt(0).Value);

            Assert.Equal(47, dictionary.ElementAt(1).Key);
            Assert.Equal(5, dictionary.ElementAt(1).Value);

            Assert.Equal(30, dictionary.ElementAt(2).Key);
            Assert.Equal(3, dictionary.ElementAt(2).Value);
        }
    }
}