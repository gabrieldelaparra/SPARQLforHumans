using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Utilities
{
    public static class StringExtensions
    {
        private static readonly Regex ToIntRegex = new Regex("[^0-9+-.,]", RegexOptions.Compiled);
        private static readonly Regex ToDoubleRegex = new Regex("[^0-9+-.,eE]", RegexOptions.Compiled);
        private static readonly Regex ToSingleLineRegex = new Regex(@"[\r\n]+", RegexOptions.Compiled);
        private const string Space = " ";

        public static int ToInt(this string input)
        {
            var value = ToIntRegex.Replace(input, string.Empty);
            return string.IsNullOrWhiteSpace(value) ? 0 : int.Parse(value);
        }

        public static double ToDouble(this string input)
        {
            var value = ToDoubleRegex.Replace(input, string.Empty);
            return string.IsNullOrWhiteSpace(value) ? 0 : double.Parse(value);
        }

        public static bool ToBool(this string input)
        {
            return !string.IsNullOrWhiteSpace(input) && bool.Parse(input);
        }

        public static string GetUriIdentifier(this string input)
        {
            var split = input.Split('/');
            return split.Any() ? split.Last() : input;
        }

        public static string ToSingleLine(this string text)
        {
            return ToSingleLineRegex.Replace(text, Space);
        }
    }
}