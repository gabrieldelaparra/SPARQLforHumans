using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SparqlForHumans.Models.Query;

namespace SparqlForHumans.Server.Controllers
{
    
    [Produces("application/json")]
    [Route("api/GraphQuery")]
    public class GraphQueryController : Controller
    {
        [HttpPost]
        public IActionResult Run([FromBody]QueryGraph graphQuery)
        {
            string results = "";
            if (graphQuery != null)
            {
                System.Console.WriteLine(graphQuery);
            }
            return Json(results);
        }
    }
}
