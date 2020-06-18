using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class BatchIdPropertyQuery : BasePropertyQuery
    {
        public BatchIdPropertyQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath,
            searchStrings) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        public override List<Property> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToProperties().Distinct().OrderByDescending(x => x.Rank).Take(totalResultsLimit)
                .ToList();
        }

        internal override bool IsInvalidSearchString(string inputString)
        {
            return string.IsNullOrEmpty(inputString);
        }
    }
}