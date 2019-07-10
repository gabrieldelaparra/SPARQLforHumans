using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IHasRange
    {
        IList<int> Range { get; set; }
    }
}