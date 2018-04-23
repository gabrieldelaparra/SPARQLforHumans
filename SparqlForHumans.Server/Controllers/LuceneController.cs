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
            var filteredItems = SearchIndex.Search(term);

            return Json(filteredItems);
        }
    }
}