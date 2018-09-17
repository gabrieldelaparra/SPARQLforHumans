using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using SparqlForHumans.Core.Properties;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
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

        // Pass SingleFieldQuery(Id), for searching by Id. Returns results sorted by rank.
        internal static Document QueryDocumentByIdAndRank(string searchId, Analyzer queryAnalyzer, Searcher searcher)
        {
            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, Labels.Id.ToString(), queryAnalyzer);

            return QueryDocumentByRank(searchId, searcher, parser);
        }

        // Pass MultiFieldQuery(Label, AltLabel), for searching Labels. Returns results sorted by rank.
        internal static Document QueryDocumentByLabelAndRank(string searchText, Analyzer queryAnalyzer, Searcher searcher, Filter filter = null)
        {
            QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                new[] { Labels.Label.ToString(), Labels.AltLabel.ToString() },
                queryAnalyzer);

            return QueryDocumentByRank(searchText, searcher, parser, filter);
        }

        //Does the search with the specific QueryParser (Label or Id). Returns results sorted by rank.
        private static Document QueryDocumentByRank(string searchText, Searcher searcher, QueryParser parser, Filter filter = null)
        {
            //Adds Sorting
            var sort = new Sort(SortField.FIELD_SCORE, new SortField(Labels.Rank.ToString(), SortField.DOUBLE, true));

            var query = BaseParser.ParseQuery(searchText, parser);
            var hit = searcher.Search(query, filter, 1, sort).ScoreDocs;

            if (hit == null || hit.Length.Equals(0))
                return null;

            return searcher.Doc(hit.FirstOrDefault().Doc);
        }
    }
}
