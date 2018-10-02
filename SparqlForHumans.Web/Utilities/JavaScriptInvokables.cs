using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Models;

namespace SparqlForHumans.Web.Utilities
{
    public class JavaScriptInvokables
    {
        public static Task<string> PromptAsync(string message)
        {
            return JSRuntime.Current.InvokeAsync<string>(
                "exampleJsFunctions.showPrompt",
                message);
        }
    }
}
