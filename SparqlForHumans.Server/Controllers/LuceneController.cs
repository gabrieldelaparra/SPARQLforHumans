using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Core.Services;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Lucene")]
    public class LuceneController : Controller
    {
        public IActionResult Autocomplete(string term)
        {
            var filteredItems = QueryService.QueryEntitiesByLabel(term);

            return Json(filteredItems);
        }
    }
}