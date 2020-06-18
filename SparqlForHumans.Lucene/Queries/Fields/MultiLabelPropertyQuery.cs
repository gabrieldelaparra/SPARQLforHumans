using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class MultiLabelPropertyQuery : BasePropertyQuery
    {
        public MultiLabelPropertyQuery(string luceneIndexPath, string searchString, int resultsLimit = 20) : base(
            luceneIndexPath, searchString, resultsLimit) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();

        internal override bool IsInvalidSearchString(string inputString)
        {
            return string.IsNullOrEmpty(inputString.ToSearchTerm());
        }

        internal override string PrepareSearchTerm(string inputString)
        {
            return ParserUtilities.PrepareSearchTerm(inputString);
        }
    }
}