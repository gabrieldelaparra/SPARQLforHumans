namespace SparqlForHumans.Models.RDFQuery
{
    public interface IQueryTriple
    {
        IQueriableSubject Subject { get; set; }
        IQueriableSubject Predicate { get; set; }
        ILabel Object { get; set; }
    }
}