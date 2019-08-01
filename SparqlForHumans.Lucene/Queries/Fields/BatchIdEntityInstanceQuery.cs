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
        public BatchIdEntityInstanceQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings, 20) { }

        internal override IQueryParser QueryParser => new InstanceOfQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
        public override List<Entity> Query()
        {
            return GetDocuments().ToEntities().Distinct(new EntityIdEqualityComparer()).OrderByDescending(x=>x.Rank).ToList();
        }
    }
}
