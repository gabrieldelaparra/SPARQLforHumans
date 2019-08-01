using System.Collections.Generic;

namespace SparqlForHumans.Models.Query
{
    public class Node : IEqualityComparer<Node>
    {
        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public bool Equals(Node x, Node y)
        {
            if (ReferenceEquals(x, y)) return true; 
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false; 
            return x.id == y.id;
        }

        public int GetHashCode(Node obj)
        {
            return id.GetHashCode();
        }
    }
}
