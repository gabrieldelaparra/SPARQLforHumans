namespace SparqlForHumans.Models.RDFQuery
{
    public interface IRDFTriple
    {
        IRDFSubject Subject { get; set; }
        IRDFSubject Predicate { get; set; }
        ILabel Object { get; set; }
    }
}