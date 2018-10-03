using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Blazor.App.Services
{
    public static class QueryServices
    {
        [JSInvokable]
        public static Task<string[]> RunQuery(string query)
        {
            return Task.FromResult(MultiDocumentQueries.QueryEntitiesByLabel(query).Select(x=>x.Label).ToArray());
        }
    }
}
