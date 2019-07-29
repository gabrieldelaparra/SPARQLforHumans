using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Utilities
{
    public static class StringExtensions
    {
        public static int ToNumbers(this string input)
        {
            var index = Regex.Replace(input, "\\D", string.Empty);
            return string.IsNullOrWhiteSpace(index) ? 0 : int.Parse(index);
        }

        public static int ToInt(this string input)
        {
            var index = Regex.Replace(input, "[^0-9+-.,]", string.Empty);
            return string.IsNullOrWhiteSpace(index) ? 0 : int.Parse(index);
        }

        public static double ToDouble(this string input)
        {
            var index = Regex.Replace(input, "[^0-9+-.,eE]", string.Empty);
            return string.IsNullOrWhiteSpace(index) ? 0 : double.Parse(index);
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
    }
}