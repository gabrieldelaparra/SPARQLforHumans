using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace SparqlForHumans.Core.Services
{
    public class QueriesParser
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
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }

            return query;
        }

        public static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            return string.Join(" ", terms);
        }
    }
}
