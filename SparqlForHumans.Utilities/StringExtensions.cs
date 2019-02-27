using System.Text.RegularExpressions;

namespace SparqlForHumans.Utilities
{
    public static class StringExtensions
    {
        public static int ToInt(this string input)
        {
            var index = Regex.Replace(input, "\\D", string.Empty);
            return int.Parse(index);
        }
    }
}
