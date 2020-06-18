using System.Linq;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using SparqlForHumans.Models.Wikidata;

namespace SparqlForHumans.Lucene.Queries.Parsers
{
    public class ParserUtilities
    {
        public static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException) {
                query = parser.Parse(QueryParserBase.Escape(searchQuery.Trim()));
            }

            return query;
        }

        public static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                .Replace(Constants.HyphenChar, Constants.BlankSpaceChar)
                .Split(Constants.BlankSpaceChar)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => $"{x.Trim()}*");

            return string.Join(Constants.QueryConcatenator, terms);
        }
    }
}