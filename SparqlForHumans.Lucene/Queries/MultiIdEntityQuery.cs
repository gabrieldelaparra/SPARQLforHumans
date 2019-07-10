using SparqlForHumans.Lucene.Queries.Base;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiIdEntityQuery : BaseEntityQuery
    {
        public MultiIdEntityQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new IdQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString);
    }
}
