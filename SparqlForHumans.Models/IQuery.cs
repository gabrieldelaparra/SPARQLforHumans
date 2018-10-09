using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IQuery
    {
        IEnumerable<ISubject> Subject { get; set; }
        IEnumerable<ISubject> Predicate { get; set; }
        IEnumerable<ILabel> Object { get; set; }
    }
}