using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BasePropertyQuery : BaseQuery<Property>
    {
        public BasePropertyQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        public BasePropertyQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(luceneIndexPath, searchString, resultsLimit) { }

        public override List<Property> Query()
        {
            return GetDocuments().ToProperties();
        }
    }
}
