using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class MultiRangePropertyQuery : BasePropertyQuery
    {
        public MultiRangePropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath,
            searchString, 20) { }

        internal override IQueryParser QueryParser => new RangeQueryParser();

        internal override bool IsInvalidSearchString(string inputString)
        {
            return string.IsNullOrEmpty(inputString.ToDigitsOnly());
        }

        internal override string PrepareSearchTerm(string inputString)
        {
            return inputString.GetUriIdentifier().ToDigitsOnly();
        }
    }
}