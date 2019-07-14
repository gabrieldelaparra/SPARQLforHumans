using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    [DataContract]
    public class QueryGraph
    {
        [DataMember] 
        public Node[] nodes { get; set; }

        [DataMember] 
        public Edge[] edges { get; set; }

        [DataMember] 
        public Selected selected { get; set; }
    }

}
