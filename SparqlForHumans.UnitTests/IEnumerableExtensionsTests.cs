using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Core.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void TestCustomGrouping()
        {
            IEnumerable<string> lines = new List<string>
            {
                "1 1",
                "1 2",
                "1 3",
                "2 1",
                "2 2",
                "2 3",
                "2 4",
                "2 5",
                "3 1",
                "4 A",
                "4 B"
            };

            Assert.Equal(11, lines.Count());

            var groups = lines.GroupByEntities();

            Assert.Equal(11, lines.Count());
            Assert.Equal(4, groups.Count());

            Assert.Equal(3, groups.Take(1).LastOrDefault().Count());
            Assert.Equal(5, groups.Take(2).LastOrDefault().Count());
            Assert.Equal(1, groups.Take(3).LastOrDefault().Count());
            Assert.Equal(2, groups.Take(4).LastOrDefault().Count());
        }

        [Fact]
        public void TestSkipFirstGroup()
        {
            IEnumerable<string> lines = new List<string>
            {
                "1 1",
                "1 2",
                "1 3",
                "2 1",
                "2 2",
                "2 3",
                "2 4",
                "2 5",
                "3 1"
            };

            Assert.Equal(9, lines.Count());

            var skippedGroup = lines.SkipFirstGroup();

            Assert.Equal(6, skippedGroup.Count());
            Assert.Equal(9, lines.Count());
        }

        [Fact]
        public void TestTakeFirstGroup()
        {
            IEnumerable<string> lines = new List<string>
            {
                "1 1",
                "1 2",
                "1 3",
                "2 1",
                "2 2",
                "2 3",
                "2 4",
                "2 5",
                "3 1"
            };

            Assert.Equal(9, lines.Count());

            var firstGroup = lines.GetFirstGroup();

            Assert.Equal(3, firstGroup.Count());
            Assert.Equal(9, lines.Count());
        }
    }
}