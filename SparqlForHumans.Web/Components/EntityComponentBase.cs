using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using SparqlForHumans.Models;

namespace SparqlForHumans.Web.Components
{
    public class EntityComponentBase 
    {
        public Entity SelectedEntity { get; set; }
    }
}
