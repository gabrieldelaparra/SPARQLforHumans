using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparqlForHumans.Blazor.App.Models
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }
}
