using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Fields
{
    public class MultiLabelTypeQuery : BaseTypeQuery
    {
        public MultiLabelTypeQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString,
            20) { }

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