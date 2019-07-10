using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiDomainPropertyQuery : BasePropertyQuery
    {
        public MultiDomainPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new DomainQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[\D]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => Regex.Replace(inputString, @"[\D]", string.Empty);
    }
}
