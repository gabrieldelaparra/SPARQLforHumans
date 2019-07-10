using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IHasDomain
    {
        IList<int> Domain { get; set; }
    }
}