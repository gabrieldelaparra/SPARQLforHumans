using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Utilities
{
    public static class DictionaryExtensions
    {
        public static void AddSafe<T1,T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, T2 value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (!dictionary[key].Contains(value))
                    dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<T2>() { value });
            }
        }

        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, (T1 key, T2 value) tuple)
        {
            dictionary.AddSafe(tuple.key, tuple.value);
        }

        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, IEnumerable<T2> values)
        {
            foreach (var value in values)
                dictionary.AddSafe(key, value);
        }

        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, (T1 key, IEnumerable<T2> values) tuple)
        {
            dictionary.AddSafe(tuple.key, tuple.values);
        }

        public static Dictionary<T2, List<T1>> InvertDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            var invertedDictionary = new Dictionary<T2, List<T1>>();

            foreach (var type in dictionary)
            foreach (var property in type.Value)
                invertedDictionary.AddSafe(property, type.Key);

            return invertedDictionary;
        }
    }
}
