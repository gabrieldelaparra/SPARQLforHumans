using SparqlForHumans.Lucene.Queries.Base;
using SparqlForHumans.Lucene.Queries.Parsers;
using System.Text.RegularExpressions;

namespace SparqlForHumans.Lucene.Queries
{
    public class SingleLabelEntityQuery : BaseEntityQuery
    {
        public SingleLabelEntityQuery(string luceneIndexPath, string searchString) : base(luceneIndexPath, searchString, 1) { }

        internal override IQueryParser QueryParser => new LabelsQueryParser();

        internal override bool IsInvalidSearchString(string inputString) => string.IsNullOrEmpty(Regex.Replace(inputString, @"[^a-zA-Z0-9-*]", string.Empty));
        internal override string PrepareSearchTerm(string inputString) => ParserUtilities.PrepareSearchTerm(inputString);
    }
}
