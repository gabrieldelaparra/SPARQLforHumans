using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    [DataContract]
    public class Node
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string[] uris { get; set; }
    }
}
