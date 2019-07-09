using Lucene.Net.Analysis.Core;
using SparqlForHumans.Models.LuceneIndex;

namespace SparqlForHumans.Lucene.Queries.Base
{
    public class IdQueryParser : BaseSingleFieldQueryParser<KeywordAnalyzer>
    {
        public override KeywordAnalyzer Analyzer => new KeywordAnalyzer();
        public override Labels FieldLabel => Labels.Id;
    }
}
