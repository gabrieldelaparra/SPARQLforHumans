using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models;
using SparqlForHumans.Models.Query;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class QueryGraphController : Controller
    {
        [HttpPost]
        public IActionResult Run([FromBody]RDFExplorerGraph graph)
        {
            var queryGraph = new QueryGraph(graph);
            queryGraph.GetGraphQueryResults(Lucene.LuceneDirectoryDefaults.EntityIndexPath, Lucene.LuceneDirectoryDefaults.PropertyIndexPath);
            var id = queryGraph.Selected.id;
            var results = queryGraph.Selected.isNode ? ToDictionary(queryGraph.Nodes[id].Results) : ToDictionary(queryGraph.Edges[id].Results);
            return Json(results);
        }
        public class Result
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }
        public Dictionary<string, Result> ToDictionary(IEnumerable<Property> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/prop/direct/{y.Id}", Value = y.Label });
        }
        public Dictionary<string, Result> ToDictionary(IEnumerable<Entity> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/entity/{y.Id}", Value = y.Label });
        }
    }


}
