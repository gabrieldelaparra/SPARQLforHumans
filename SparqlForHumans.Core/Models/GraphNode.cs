using System.Collections.Generic;

namespace SparqlForHumans.Core.Models
{
    public class GraphNode
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public List<string> ConnectedNodes { get; set; } = new List<string>();
        public double Rank { get; set; }

        public GraphNode(string id, int index)
        {
            Id = id;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Id} - {ConnectedNodes.Count}";
        }
    }
}