using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.UnitTests
{
    public static class DictionaryExtensions
    {
        public static void AddSafe(this Dictionary<int, List<int>> internalDictionary, int key, int value)
        {
            if (internalDictionary.ContainsKey(key))
                internalDictionary[key].Add(value);
            else
                internalDictionary.Add(key, new List<int>() { value });
        }
    }
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
                var triple = line.GetTriple();
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
    }
}
