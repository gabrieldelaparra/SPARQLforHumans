using SparqlForHumans.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void TestTakeFirstGroup()
        {
            IEnumerable<string> lines = new List<string>()
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
            };

            Assert.Equal(9, lines.Count());

            var firstGroup = lines.GetFirstGroup();

            Assert.Equal(3, firstGroup.Count());
            Assert.Equal(9, lines.Count());
        }

        [Fact]
        public void TestSkipFirstGroup()
        {
            IEnumerable<string> lines = new List<string>()
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
            };

            Assert.Equal(9, lines.Count());

            var skippedGroup = lines.SkipFirstGroup();
            
            Assert.Equal(6, skippedGroup.Count());
            Assert.Equal(9, lines.Count());
        }

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
            };

            Assert.Equal(9, lines.Count());

            var firstGroup = lines.GetFirstGroup();

            Assert.Equal(9, lines.Count());
            Assert.Equal(3, firstGroup.Count());

            lines = lines.SkipFirstGroup();

            Assert.Equal(6, lines.Count());
        }
    }
}
