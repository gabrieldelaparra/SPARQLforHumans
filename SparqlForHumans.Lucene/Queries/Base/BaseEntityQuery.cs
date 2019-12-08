using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BaseEntityQuery : BaseQuery<Entity>
    {
        public BaseEntityQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        public BaseEntityQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(luceneIndexPath, searchString, resultsLimit) { }

        public override List<Entity> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToEntities();
        }
    }
}
