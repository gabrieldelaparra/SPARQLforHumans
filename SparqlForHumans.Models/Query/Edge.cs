using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    [DataContract]
    public class Edge
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string[] uris { get; set; }

        [DataMember]
        public int sourceId { get; set; }

        [DataMember]
        public int targetId { get; set; }
    }
}
