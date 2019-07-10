using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiLabelPropertyQuery : BasePropertyQuery
    {
        public MultiLabelPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[^a-zA-Z0-9-*]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => ParserUtilities.PrepareSearchTerm(inputString);
    }

        public class MultiDomainPropertyQuery : BasePropertyQuery
    {
        public MultiDomainPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 20) { }

        internal override IQueryParser QueryParser => new DomainQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[\D]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => inputString;
    }
}
