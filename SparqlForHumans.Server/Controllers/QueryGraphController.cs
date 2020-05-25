using System;
using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SparqlForHumans.Lucene;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class QueryGraphController : Controller
    {
        private readonly NLog.Logger _logger = Logger.Logger.Init();
        [HttpPost]
        public IActionResult Run([FromBody]RDFExplorerGraph graph)
        {
            _logger.Info("Query Start:");
            _logger.Info($"Incoming Graph: {graph}");
            var queryGraph = new QueryGraph(graph);
            new QueryGraphResults().GetGraphQueryResults(queryGraph, LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            _logger.Info("Query End");
            return Json(queryGraph);
        }
        public class Result
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }
        //public Dictionary<string, Result> ToDictionary(IEnumerable<Property> subjects)
        //{
        //    return subjects.ToDictionary(x => x.Id, y => new Result() { Id = $"http://www.wikidata.org/prop/direct/{y.Id}", Value = y.Label });
        //}
        //public Dictionary<string, Result> ToDictionary(IEnumerable<Entity> subjects)
        //{
        //    return subjects.ToDictionary(x => x.Id, y => new Result() { Id = $"http://www.wikidata.org/entity/{y.Id}", Value = y.Label });
        //}
    }


}
