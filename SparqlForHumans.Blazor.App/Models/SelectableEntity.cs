using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparqlForHumans.Models;

namespace SparqlForHumans.Blazor.App.Models
{
    public class SelectableEntity : Entity, ISelectable
    {
        public bool IsSelected { get; set; }

        public new List<SelectableProperty> Properties { get; set; } = new List<SelectableProperty>();
    }
}
