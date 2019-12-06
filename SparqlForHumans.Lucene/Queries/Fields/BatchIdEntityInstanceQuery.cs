using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries
{
    public class BatchIdEntityInstanceQuery : BaseEntityQuery
    {
        public BatchIdEntityInstanceQuery(string luceneIndexPath, IEnumerable<string> searchStrings, int resultsLimit = 20) : base(luceneIndexPath, searchStrings, resultsLimit) { }

        internal override IQueryParser QueryParser => new InstanceOfQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
        public override List<Entity> Query(int resultsLimit = 100)
        {
            return GetDocuments().ToEntities().Distinct(new EntityIdEqualityComparer()).OrderByDescending(x => x.Rank).Take(resultsLimit).ToList();
        }
    }
}
