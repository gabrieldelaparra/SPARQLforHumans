using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Models.RDFExplorer;

namespace SparqlForHumans.Lucene.Models
{
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph)
        {
            Nodes = new SortedDictionary<int, QueryNode>(rdfGraph.nodes.Distinct()
                .ToDictionary(x => x.id, x => new QueryNode(x)));
            Edges = new SortedDictionary<int, QueryEdge>(rdfGraph.edges.Distinct()
                .ToDictionary(x => x.id, x => new QueryEdge(x)));

            this.CheckNodeTypes();
        }

        public SortedDictionary<int, QueryEdge> Edges { get; set; }
        [JsonIgnore] public string EntitiesIndexPath { get; set; }
        public SortedDictionary<int, QueryNode> Nodes { get; set; }
        [JsonIgnore] public string PropertiesIndexPath { get; set; }

        public override bool Equals(object obj)
        {
            var y = obj as QueryGraph;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return Nodes.SequenceEqual(y.Nodes) && Edges.SequenceEqual(y.Edges);
        }

        public override int GetHashCode()
        {
            //Not the best practice, but effective.
            return ToString().GetHashCode();
        }

        public void SetIndexPaths(string entitiesIndexPath, string propertiesIndexPath)
        {
            EntitiesIndexPath = entitiesIndexPath;
            PropertiesIndexPath = propertiesIndexPath;
        }

        public override string ToString()
        {
            var nodesString = $"{{ Nodes: {{{string.Join("; ", Nodes.Select(x => x.Value.ToString()))}}} }}";
            var edgesString = $"{{ Edges: {{{string.Join("; ", Edges.Select(x => x.Value.ToString()))}}} }}";
            return $"{nodesString} {edgesString}";
        }
    }
}