namespace SparqlForHumans.Models.RDFQuery
{
    public interface IRDFTriple
    {
        IHasURI Subject { get; set; }
        IHasURI Predicate { get; set; }
        IHasLabel Object { get; set; }
    }
}