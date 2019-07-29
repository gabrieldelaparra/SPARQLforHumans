using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Models.Query;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/QueryGraph")]
    public class QueryGraphController : Controller
    {
        [HttpPost]
        public IActionResult Run([FromBody]RDFExplorerGraph queryGraph)
        {
            string results = "";
            if (queryGraph != null)
            {
                System.Console.WriteLine(queryGraph);
            }
            return Json(results);
        }
    }
}
