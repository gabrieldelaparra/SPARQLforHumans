using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IHasInstanceOf
    {
        IList<string> ParentTypes { get; set; }
    }
}