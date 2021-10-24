using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BasePropertyQuery : BaseQuery<Property>
    {
        public BasePropertyQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) :
            base(luceneIndexPath, searchStrings, resultsLimit) { }

        public BasePropertyQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(
            luceneIndexPath, searchString, resultsLimit) { }

        public override IEnumerable<Property> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToProperties().Distinct().OrderByDescending(x => x.Rank).Take(totalResultsLimit);
        }
    }
}