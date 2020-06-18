using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class SingleIdPropertyQuery : BasePropertyQuery
    {
        public SingleIdPropertyQuery(string luceneIndexPath, string searchString) :
            base(luceneIndexPath, searchString) { }

        internal override IQueryParser QueryParser => new IdQueryParser();

        internal override bool IsInvalidSearchString(string inputString)
        {
            return string.IsNullOrEmpty(inputString);
        }
    }
}