using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Utilities
{
    public static class DictionaryExtensions
    {
        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, T2 value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (!dictionary[key].Contains(value))
                    dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<T2> { value });
            }
        }

        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, IEnumerable<T2> values)
        {
            foreach (var value in values)
                dictionary.AddSafe(key, value);

            if (dictionary.ContainsKey(key))
                dictionary[key].TrimExcess();
        }

        public static Dictionary<T2, List<T1>> InvertDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            var invertedDictionary = new Dictionary<T2, List<T1>>();

            foreach (var type in dictionary)
                foreach (var property in type.Value)
                    invertedDictionary.AddSafe(property, type.Key);

            return invertedDictionary;
        }

        public static Dictionary<T2, T1[]> InvertDictionary<T1, T2>(this Dictionary<T1, T2[]> dictionary)
        {
            var invertedDictionary = new Dictionary<T2, List<T1>>();

            foreach (var type in dictionary)
                foreach (var property in type.Value)
                    invertedDictionary.AddSafe(property, type.Key);

            return invertedDictionary.ToArrayDictionary();
        }

        public static Dictionary<T1, T2[]> ToArrayDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            return dictionary.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public static void TrimExcess<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            foreach (var pair in dictionary)
            {
                pair.Value.TrimExcess();
            }
            dictionary.TrimExcess();
        }
    }
}