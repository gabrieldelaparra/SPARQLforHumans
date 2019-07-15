using System.Linq;
using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    public class QueryGraph
    {
        public Node[] nodes { get; set; }

        public Edge[] edges { get; set; }

        public Selected selected { get; set; }

        public override string ToString()
        {
            return $"{string.Join<Node>(";", nodes)}";
        }
    }

}
