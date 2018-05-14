using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Lucene")]
    public class LuceneController : Controller
    {
        public IActionResult Autocomplete(string term)
        {
            var filteredItems = QueryService.QueryByLabel(term);

            var typeLabels = new Dictionary<string, string>();

            foreach (var item in filteredItems)
            {
                if (item.Type == null) continue;

                if (typeLabels.ContainsKey(item.Type))
                {
                    item.TypeLabel = typeLabels.FirstOrDefault(x => x.Key.Equals(item.Type)).Value;
                }
                else
                {
                    var typeLabel = QueryService.GetTypeLabel(item.Type);
                    if (typeLabel.Any())
                    {
                        typeLabels.Add(item.Type, typeLabel.FirstOrDefault().Label);
                        item.TypeLabel = typeLabel.FirstOrDefault().Label;
                    }
                    else
                    {
                        item.TypeLabel = "Not Found";
                    }
                }
            }

            return Json(filteredItems);
        }
    }
}