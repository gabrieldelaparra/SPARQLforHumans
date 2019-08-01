using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Models.Query
{
    public class RDFExplorerGraph : IEqualityComparer<RDFExplorerGraph>
    {
        public Node[] nodes { get; set; } = new Node[0];

        public Edge[] edges { get; set; } = new Edge[0];

        public Selected selected { get; set; } = new Selected();

        public override string ToString()
        {
            return $"{string.Join<Node>(";", nodes)}";
        }

        public bool Equals(RDFExplorerGraph x, RDFExplorerGraph y)
        {
            if (ReferenceEquals(x, y)) return true; 
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            return x.nodes.SequenceEqual(y.nodes) && x.edges.SequenceEqual(y.edges);
        }

        public int GetHashCode(RDFExplorerGraph obj)
        {
            return nodes.GetHashCode() ^ edges.GetHashCode();
        }
    }


}
