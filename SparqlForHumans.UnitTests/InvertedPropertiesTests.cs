using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class InvertedPropertiesTests
    {
        public int getMax(int current, int compare)
        {
            return current <= compare ? compare : current;
        }

        public int[][] GetInvertedIndex(string triplesFilename, int entitiesCount)
        {
            var lines = FileHelper.GetInputLines(triplesFilename);

            var maxEntities = 1;
            var nodeArray = new Dictionary<int, List<int>>();

            foreach (var line in lines)
            {
                var triple = line.ToTriple();
                var s = triple.Subject;
                var p = triple.Predicate;
                var o = triple.Object;

                if (!s.IsEntityQ())
                    continue;

                if (!p.IsProperty())
                    continue;

                if (!o.IsEntityQ())
                    continue;

                var sId = s.GetIntId();
                var oId = o.GetIntId();

                nodeArray.AddSafe(oId, sId);
            }

            return null;
        }

        [Fact]
        public void TestCreateEntityType2DArray()
        {
        }
    }
}