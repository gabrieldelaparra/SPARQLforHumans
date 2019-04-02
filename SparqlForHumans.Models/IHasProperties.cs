using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IHasProperties<T>
    {
        IList<T> Properties { get; set; }
    }
}