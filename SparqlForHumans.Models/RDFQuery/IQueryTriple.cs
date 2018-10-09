using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IQueryTriple
    {
        ISubject Subject { get; set; }
        ISubject Predicate { get; set; }
        ILabel Object { get; set; }
    }
}