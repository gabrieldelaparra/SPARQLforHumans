using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Queries
{
    public class BatchIdEntityInstanceQuery : BaseEntityQuery
    {
        public BatchIdEntityInstanceQuery(string luceneIndexPath, IEnumerable<string> searchStrings) : base(luceneIndexPath, searchStrings) { }

        internal override IQueryParser QueryParser => new InstanceOfQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }
}
