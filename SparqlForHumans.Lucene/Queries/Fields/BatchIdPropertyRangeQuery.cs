using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class BatchIdPropertyRangeQuery : BasePropertyQuery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="luceneIndexPath">Properties Index Path</param>
        /// <param name="searchStrings">Entities Index "Qxx" array of Types only</param>
        public BatchIdPropertyRangeQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings, 20) { }

        internal override IQueryParser QueryParser => new RangeQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[\D]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => Regex.Replace(inputString, @"[\D]", string.Empty);
        public override List<Property> Query(int resultsLimit = 100)
        {
            return GetDocuments().ToProperties().Distinct().OrderByDescending(x => x.Rank).Take(resultsLimit).ToList();
        }
    }
}
