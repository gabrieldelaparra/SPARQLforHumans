﻿using Microsoft.AspNetCore.Mvc;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Lucene;
using SparqlForHumans.Lucene.Models;
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
            StaticQueryGraphResults.QueryGraphResults.GetGraphQueryResults(queryGraph, LuceneDirectoryDefaults.EntityIndexPath, LuceneDirectoryDefaults.PropertyIndexPath);
            _logger.Info("Query End");
            return Json(queryGraph);
        }
        public class Result
        {
            public string Id { get; set; }
            public string Value { get; set; }
        }
    }


}
