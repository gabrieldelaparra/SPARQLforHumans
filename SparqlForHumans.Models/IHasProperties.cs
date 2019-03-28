using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models
{
    public interface IHasProperties<T>
    {
        IList<T> Properties { get; set; }
    }
}
