using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models;
using SparqlForHumans.Models.Query;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/QueryGraph")]
    public class QueryGraphController : Controller
    {
        [HttpPost]
        public IActionResult Run([FromBody]RDFExplorerGraph graph)
        {
            var queryGraph = new QueryGraph(graph, Lucene.LuceneDirectoryDefaults.EntityIndexPath, Lucene.LuceneDirectoryDefaults.PropertyIndexPath);
            queryGraph.RunGraphQueryResults();
            var selectedId = queryGraph.Selected.id;
            var results = queryGraph.Selected.isNode ? ToDictionary(queryGraph.Nodes[queryGraph.Selected.id].Results) : ToDictionary(queryGraph.Edges[queryGraph.Selected.id].Results);
            return Json(results);
        }
        public class Result
        {
            public string Id { get; set; }
            public string Label { get; set; }
        }
        public Dictionary<string, Result> ToDictionary(IEnumerable<Property> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/prop/direct/{y.Id}", Label = y.Label });
        }
        public Dictionary<string, Result> ToDictionary(IEnumerable<Entity> subjects)
        {
            return subjects.ToDictionary(x => x.Id, y => new Result(){Id = $"http://www.wikidata.org/entity/{y.Id}", Label = y.Label });
        }
    }


}
