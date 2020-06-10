namespace SparqlForHumans.Lucene.Models
{
    public class Result
    {
        public string Type => "uri";
        public string Text { get; set; }
        public string Value { get; set; }
    }
}
