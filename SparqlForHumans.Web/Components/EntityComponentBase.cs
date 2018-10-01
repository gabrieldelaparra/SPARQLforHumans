using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using SparqlForHumans.Models;

namespace SparqlForHumans.Web.Components
{
    public class EntityComponentBase
    {
        [Parameter]
        public string Header { get; set; } = string.Empty;

        public Property[] properties = new Property[0];

        int i = 0;
        public void addProperty()
        {
            properties = properties.Append(new Property()
            {
                Id = i++.ToString(),
            }).ToArray();
        }


        public void removeProperty()
        {
            properties = properties.ToList().Take(properties.Length - 1).ToArray();
        }
    }
}
