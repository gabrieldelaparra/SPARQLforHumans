using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Core.EF
{
    public class GraphNode
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public virtual ICollection<ConnectedNode> ConnectedNodes { get; set; } = new List<ConnectedNode>();
        public double Rank { get; set; }

        public GraphNode() { }

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

    public class ConnectedNode
    {
        public ConnectedNode() { }

        public ConnectedNode(string value) => Value = value;

        public int Id { get; set; }
        public string Value { get; set; }
    }
}
