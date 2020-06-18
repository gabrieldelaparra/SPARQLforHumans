using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiRangePropertyQuery : BasePropertyQuery
    {
        public MultiRangePropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new RangeQueryParser();
        internal override bool IsInvalidSearchString(string inputString) =>  string.IsNullOrEmpty(inputString.ToDigitsOnly());
        internal override string PrepareSearchTerm(string inputString) => inputString.GetUriIdentifier().ToDigitsOnly();
    }
}
