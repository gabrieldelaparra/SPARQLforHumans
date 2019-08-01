using System.Collections.Generic;

namespace SparqlForHumans.Models.Query
{
    public class Edge : IEqualityComparer<Edge>
    {
        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public int sourceId { get; set; }

        public int targetId { get; set; }

        public bool Equals(Edge x, Edge y)
        {
            if (ReferenceEquals(x, y)) return true; 
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false; 
            return x.id == y.id;
        }

        public int GetHashCode(Edge obj)
        {
            return id.GetHashCode();
        }
    }
}
