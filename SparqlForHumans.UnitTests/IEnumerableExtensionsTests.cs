using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Extensions;
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

            var groups = lines.GroupBySubject();

            Assert.Equal(11, lines.Count());
            Assert.Equal(4, groups.Count());

            Assert.Equal(3, groups.Take(1).LastOrDefault().Count());
            Assert.Equal(5, groups.Take(2).LastOrDefault().Count());
            Assert.Single(groups.Take(3).LastOrDefault());
            Assert.Equal(2, groups.Take(4).LastOrDefault().Count());
        }
    }
}