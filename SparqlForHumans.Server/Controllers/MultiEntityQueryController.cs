using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/MultiEntityQuery")]
    public class MultiEntityQueryController : Controller
    {
        public IActionResult Run(string term)
        {
            var filteredItems = new MultiLabelEntityQuery(LuceneDirectoryDefaults.EntityIndexPath, term).Query();
            //filteredItems.AddProperties();

            return Json(filteredItems);
        }
    }
}