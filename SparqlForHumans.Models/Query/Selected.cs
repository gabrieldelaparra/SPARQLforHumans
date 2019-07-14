using System.Runtime.Serialization;

namespace SparqlForHumans.Models.Query
{
    [DataContract]
    public class Selected
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public bool isNode { get; set; }
    }
}
