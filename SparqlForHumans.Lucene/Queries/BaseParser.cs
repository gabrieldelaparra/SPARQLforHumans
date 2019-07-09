using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using SparqlForHumans.Models.Wikidata;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries
{
    public class BaseParser
    {
        public static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParserBase.Escape(searchQuery.Trim()));
            }

            return query;
        }

        public static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                .Replace(WikidataDump.HyphenChar, WikidataDump.BlankSpaceChar)
                .Split(WikidataDump.BlankSpaceChar)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => $"{x.Trim()}*");

            return string.Join(WikidataDump.QueryConcatenator, terms);
        }
    }
}