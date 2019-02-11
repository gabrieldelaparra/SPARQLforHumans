using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Utilities
{
    public static class DictionaryExtensions
    {
        public static void AddSafe<T>(this Dictionary<T, List<T>> internalDictionary, T key, T value)
        {
            if (internalDictionary.ContainsKey(key))
                internalDictionary[key].Add(value);
            else
                internalDictionary.Add(key, new List<T>() { value });
        }
    }
}
