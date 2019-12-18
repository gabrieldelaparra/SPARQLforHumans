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
                dictionary[key].AddSafe(value);
            }
            else
            {
                dictionary.Add(key, new List<T2> { value });
            }
        }

        public static void AddSafe<T1, T2>(this Dictionary<T1, List<T2>> dictionary, T1 key, IEnumerable<T2> values)
        {
            if (!values.Any())
            {
                return;
            }

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = dictionary[key].Union(values).ToList();
            }
            else
            {
                dictionary.Add(key, values.Distinct().ToList());
            }
        }
        public static void AddSafe<T1, T2>(this Dictionary<T1, HashSet<T2>> dictionary, T1 key, IEnumerable<T2> values)
        {
            if (!values.Any())
                return;

            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new HashSet<T2>());

            foreach (var value in values)
                dictionary[key].Add(value);
        }


        public static Dictionary<T2, List<T1>> InvertDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            var invertedDictionary = new Dictionary<T2, List<T1>>();

            foreach (var type in dictionary)
            {
                foreach (var property in type.Value)
                {
                    invertedDictionary.AddSafe(property, type.Key);
                }
            }

            invertedDictionary.TrimExcessDeep();
            return invertedDictionary;
        }

        public static Dictionary<T2, T1[]> InvertDictionary<T1, T2>(this Dictionary<T1, T2[]> dictionary)
        {
            var invertedDictionary = new Dictionary<T2, List<T1>>();

            foreach (var type in dictionary)
            {
                foreach (var property in type.Value)
                {
                    invertedDictionary.AddSafe(property, type.Key);
                }
            }

            return invertedDictionary.ToArrayDictionary();
        }

        public static Dictionary<T1, T2[]> ToArrayDictionary<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            dictionary.TrimExcessDeep();
            return dictionary.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public static Dictionary<T1, T2[]> ToArrayDictionary<T1, T2>(this Dictionary<T1, HashSet<T2>> dictionary)
        {
            return dictionary.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public static void TrimExcessDeep<T1, T2>(this Dictionary<T1, List<T2>> dictionary)
        {
            foreach (var pair in dictionary)
            {
                pair.Value.TrimExcess();
            }
        }

        //public static void Print<T1, T2>(this Dictionary<T1, T2[]> dictionary)
        //{
        //    foreach (var pair in dictionary)
        //    {
        //        Console.WriteLine($"{pair.Key}:{string.Join(" ", pair.Value)}");
        //    }
        //}
    }
}