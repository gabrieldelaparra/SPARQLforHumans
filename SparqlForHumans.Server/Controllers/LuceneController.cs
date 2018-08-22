using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Lucene")]
    public class LuceneController : Controller
    {
        /*
         * TODO: Al hacer el autoComplete, me muestra las propiedades y las entidades.
         * Debería mostrarme solo las propiedes supongo. No?
         */
        public IActionResult Autocomplete(string term)
        {
            var filteredItems = QueryService.QueryEntitiesByLabel(term, LuceneIndexExtensions.LuceneIndexDirectory);
            filteredItems = filteredItems.Select(x => x.AddProperties(LuceneIndexExtensions.LuceneIndexDirectory));

            return Json(filteredItems);
        }
    }
}