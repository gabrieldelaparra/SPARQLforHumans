using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Lucene.Queries.Fields;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class MultiEntityQueryController : Controller
    {
        public IActionResult Run(string term)
        {
            var filteredItems = new MultiLabelEntityQuery(LuceneDirectoryDefaults.EntityIndexPath, term).Query();
            return Json(filteredItems);
        }
    }
}