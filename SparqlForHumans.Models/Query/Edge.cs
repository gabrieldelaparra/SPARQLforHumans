using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    public class Edge
    {
        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public int sourceId { get; set; }

        public int targetId { get; set; }
    }
}
