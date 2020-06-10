namespace SparqlForHumans.Lucene.Models
{
    public class Result
    {
        public string Text { get; set; }
        public string Type => "uri";
        public string Value { get; set; }
    }
}