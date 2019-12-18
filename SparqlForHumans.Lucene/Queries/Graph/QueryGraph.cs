using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph)
        {
            Nodes = rdfGraph.nodes.Distinct().ToDictionary(x => x.id, x => new QueryNode(x));
            Edges = rdfGraph.edges.Distinct().ToDictionary(x => x.id, x => new QueryEdge(x));

            this.CheckNodeTypes();
        }

        public void SetIndexPaths(string entitiesIndexPath, string propertiesIndexPath)
        {
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertiesIndexPath;
        }

        public string EntitiesIndexPath { get; set; }
        public string PropertiesIndexPath { get; set; }

        public Dictionary<int, QueryNode> Nodes { get; set; }
        public Dictionary<int, QueryEdge> Edges { get; set; }

        public override string ToString()
        {
            var nodesString = $"{{ Nodes: {{{string.Join("; ", Nodes.Select(x=>x.ToString()))}}} }}";
            var edgesString = $"{{ Edges: {{{string.Join("; ", Edges.Select(x=>x.ToString()))}}} }}";
            return $"{nodesString} {edgesString}";
        }
    }
}
