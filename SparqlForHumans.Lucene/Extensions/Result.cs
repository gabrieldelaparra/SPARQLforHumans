namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static partial class QueryGraphExtensions
    {
        public class Result
        {
            public string Type => "uri";
            public string Text { get; set; }
            public string Value { get; set; }
        }
    }
}
