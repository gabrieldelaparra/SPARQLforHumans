using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparqlForHumans.Models;

namespace SparqlForHumans.Blazor.App.Models
{
    public class SelectableProperty : Property, ISelectable
    {
        public bool IsSelected { get; set; }
    }
}
