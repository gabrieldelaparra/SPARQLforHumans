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
        public static Task<int[]> ReturnArrayAsync()
        {
            return Task.FromResult(new int[] { 1, 2, 3 });
        }
    }
}
