using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    public class Node
    {
        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];
    }
}
