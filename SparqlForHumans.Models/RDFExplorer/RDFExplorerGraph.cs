using System;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Models.RDFExplorer
{
    public class RDFExplorerGraph
    {
        public Node[] nodes { get; set; } = new Node[0];

        public Edge[] edges { get; set; } = new Edge[0];

        public Selected selected { get; set; } = new Selected();

        public override bool Equals(object obj)
        {
            var y = obj as RDFExplorerGraph;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return this.nodes.SequenceEqual(y.nodes) && this.edges.SequenceEqual(y.edges);
        }

        public override int GetHashCode()
        {
            return this.nodes.GetHashCode() ^ this.edges.GetHashCode();
        }

        public override string ToString()
        {
            var nodesString = $"{{ Nodes: {{{string.Join("; ", nodes.Select(x=>x.ToString()))}}} }}";
            var edgesString = $"{{ Edges: {{{string.Join("; ", edges.Select(x=>x.ToString()))}}} }}";
            return $"{nodesString} {edgesString}";
        }

    }
}
