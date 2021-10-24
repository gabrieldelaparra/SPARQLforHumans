using System.Collections.Generic;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public abstract class BaseEntityQuery : BaseQuery<Entity>
    {
        public BaseEntityQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 1) : base(
            luceneIndexPath, searchStrings, resultsLimit) { }

        public BaseEntityQuery(string luceneIndexPath, string searchString, int resultsLimit = 1) : base(
            luceneIndexPath, searchString, resultsLimit) { }

        public override IEnumerable<Entity> Query(int totalResultsLimit = 100)
        {
            return GetDocuments().ToEntities();
        }
    }
}