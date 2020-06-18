using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries
{
    public class SingleLabelPropertyQuery : BasePropertyQuery
    {
        public SingleLabelPropertyQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(inputString.ToSearchTerm());
        internal override string PrepareSearchTerm(string inputString) => ParserUtilities.PrepareSearchTerm(inputString);
    }
}
