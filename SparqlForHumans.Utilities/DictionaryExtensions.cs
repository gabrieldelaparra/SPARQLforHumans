using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Utilities
{
    public static class DictionaryExtensions
    {
        public static void AddSafe<T>(this Dictionary<T, List<T>> dictionary, T key, T value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (!dictionary[key].Contains(value))
                    dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<T>() { value });
            }
        }

        public static Dictionary<T, List<T>> InvertDictionary<T>(this Dictionary<T, List<T>> dictionary)
        {
            var invertedDictionary = new Dictionary<T, List<T>>();

            foreach (var type in dictionary)
            foreach (var property in type.Value)
                invertedDictionary.AddSafe(property, type.Key);

            return invertedDictionary;
        }
    }
}
