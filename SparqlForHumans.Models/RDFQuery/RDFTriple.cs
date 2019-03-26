namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFTriple
    {
        public RDFEntity Subject { get; set; }
        public RDFProperty Predicate { get; set; }
        public RDFEntity Object { get; set; }
    }
}