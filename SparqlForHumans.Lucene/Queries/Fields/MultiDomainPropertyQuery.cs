using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class MultiDomainPropertyQuery : BasePropertyQuery
    {
        public MultiDomainPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath,
            searchString, 20) { }

        internal override IQueryParser QueryParser => new DomainQueryParser();

        internal override bool IsInvalidSearchString(string inputString) =>
            string.IsNullOrEmpty(inputString.ToDigitsOnly());

        internal override string PrepareSearchTerm(string inputString) => inputString.GetUriIdentifier().ToDigitsOnly();
    }
}