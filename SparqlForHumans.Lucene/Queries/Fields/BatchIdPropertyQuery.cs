using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries
{
    public class BatchIdPropertyQuery : BasePropertyQuery
    {
        public BatchIdPropertyQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
        public override List<Property> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToProperties().Distinct().OrderByDescending(x=>x.Rank).Take(totalResultsLimit).ToList();
        }
    }
}
