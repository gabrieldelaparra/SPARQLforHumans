using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using Xunit;

namespace SparqlForHumans.UnitTests.Utilities
{
    public class EnumerableExtensionsTests
    {
        [Fact]
        public void TestCustomGrouping()
        {
            IEnumerable<string> lines = new List<string>
            {
                "<http://subject/1> <http://predicate/1> <http://object/x> .",
                "<http://subject/1> <http://predicate/2> <http://object/x> .",
                "<http://subject/1> <http://predicate/3> <http://object/x> .",
                "<http://subject/2> <http://predicate/1> <http://object/x> .",
                "<http://subject/2> <http://predicate/2> <http://object/x> .",
                "<http://subject/2> <http://predicate/3> <http://object/x> .",
                "<http://subject/2> <http://predicate/4> <http://object/x> .",
                "<http://subject/2> <http://predicate/5> <http://object/x> .",
                "<http://subject/3> <http://predicate/1> <http://object/x> .",
                "<http://subject/4> <http://predicate/A> <http://object/x> .",
                "<http://subject/4> <http://predicate/B> <http://object/x> ."
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