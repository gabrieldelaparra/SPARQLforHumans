using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class DictionaryExtensionsTests
    {
        [Fact]
        public void TestAddSafeDifferentTypes()
        {
            var dictionary = new Dictionary<int, List<string>>();

            dictionary.AddSafe(1, "1");
            Assert.Single(dictionary[1]);
            Assert.Equal("1", dictionary[1].ElementAt(0));

            dictionary.AddSafe(1, "2");
            Assert.Equal(2, dictionary[1].Count);
            Assert.Equal("1", dictionary[1].ElementAt(0));
            Assert.Equal("2", dictionary[1].ElementAt(1));
        }

        [Fact]
        public void TestAddSafeRangeDifferentTypes()
        {
            var dictionary = new Dictionary<int, List<string>>();

            dictionary.AddSafe(1, new List<string> { "1" });
            Assert.Single(dictionary[1]);
            Assert.Equal("1", dictionary[1].ElementAt(0));

            dictionary.AddSafe(1, new List<string> { "1", "2", "3" });
            Assert.Equal(3, dictionary[1].Count);
            Assert.Equal("1", dictionary[1].ElementAt(0));
            Assert.Equal("2", dictionary[1].ElementAt(1));
            Assert.Equal("3", dictionary[1].ElementAt(2));
        }

        [Fact]
        public void TestAddSafeRangeSameType()
        {
            var dictionary = new Dictionary<int, List<int>>();

            dictionary.AddSafe(1, new List<int> { 1 });
            Assert.Single(dictionary[1]);
            Assert.Equal(1, dictionary[1].ElementAt(0));

            dictionary.AddSafe(1, new List<int> { 1, 2, 3 });
            Assert.Equal(3, dictionary[1].Count);
            Assert.Equal(1, dictionary[1].ElementAt(0));
            Assert.Equal(2, dictionary[1].ElementAt(1));
            Assert.Equal(3, dictionary[1].ElementAt(2));
        }

        [Fact]
        public void TestAddSafeSameType()
        {
            var dictionary = new Dictionary<int, List<int>>();

            dictionary.AddSafe(1, 1);
            Assert.Single(dictionary[1]);
            Assert.Equal(1, dictionary[1].ElementAt(0));

            dictionary.AddSafe(1, 2);
            Assert.Equal(2, dictionary[1].Count);
            Assert.Equal(1, dictionary[1].ElementAt(0));
            Assert.Equal(2, dictionary[1].ElementAt(1));
        }

        [Fact]
        public void TestInvertDictionaryDifferentTypes()
        {
            var dictionary = new Dictionary<int, List<string>>();
            dictionary.AddSafe(1, new List<string> { "1", "2", "3" });
            Assert.Single(dictionary);
            Assert.Equal(3, dictionary[1].Count);
            var inverted = dictionary.InvertDictionary();
            Assert.Equal(3, inverted.Count);
            Assert.Single(inverted["1"]);
            Assert.Equal(1, inverted["1"].ElementAt(0));
            Assert.Single(inverted["2"]);
            Assert.Equal(1, inverted["2"].ElementAt(0));
            Assert.Single(inverted["3"]);
            Assert.Equal(1, inverted["3"].ElementAt(0));
        }

        [Fact]
        public void TestInvertDictionarySameType()
        {
            var dictionary = new Dictionary<int, List<int>>();
            dictionary.AddSafe(1, new List<int> { 1, 2, 3 });
            Assert.Single(dictionary);
            Assert.Equal(3, dictionary[1].Count);
            var inverted = dictionary.InvertDictionary();
            Assert.Equal(3, inverted.Count);
            Assert.Single(inverted[1]);
            Assert.Equal(1, inverted[1].ElementAt(0));
            Assert.Single(inverted[2]);
            Assert.Equal(1, inverted[2].ElementAt(0));
            Assert.Single(inverted[3]);
            Assert.Equal(1, inverted[3].ElementAt(0));
        }

        [Fact]
        public void TestToArrayDictionary()
        {
            var dictionary = new Dictionary<int, List<int>>();
            dictionary.AddSafe(1, new List<int> { 1, 2, 3 });
            Assert.Single(dictionary);
            Assert.Equal(3, dictionary[1].Count);
            var arrayDictionary = dictionary.ToArrayDictionary();
            Assert.Single(dictionary);
            Assert.Equal(1, dictionary[1][0]);
            Assert.Equal(2, dictionary[1][1]);
            Assert.Equal(3, dictionary[1][2]);
        }
    }
}