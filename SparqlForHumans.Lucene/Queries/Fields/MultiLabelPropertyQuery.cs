using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class MultiLabelPropertyQuery : BasePropertyQuery
    {
        public MultiLabelPropertyQuery(string luceneIndexPath, string searchString, int resultsLimit = 20) : base(luceneIndexPath, searchString, resultsLimit) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();
        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[^a-zA-Z0-9-*]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => ParserUtilities.PrepareSearchTerm(inputString);
    }
}
