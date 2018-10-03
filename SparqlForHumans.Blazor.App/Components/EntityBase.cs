using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using SparqlForHumans.Blazor.App.Shared;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Blazor.App.Components
{
    public class EntityBase : BlazorComponent
    {
        [JSInvokable]
        public static Task<SelectableValue[]> RunQuery(string query)
        {
            return Task.FromResult(MultiDocumentQueries.QueryEntitiesByLabel(query)
                .Select(x => new SelectableValue() { Label = x.Label, Value = x }).ToArray());
        }

        [JSInvokable]
        public static void SelectionChanged(object selectedObject)
        {

        }
    }
}
