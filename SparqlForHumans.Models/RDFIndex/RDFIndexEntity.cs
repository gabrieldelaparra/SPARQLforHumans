using System.Collections.Generic;

namespace SparqlForHumans.Models.RDFIndex
{
    public class RDFIndexEntity : BaseEntity, IHasProperties<string>, IHasRank<double>
    {
        //Constructor
        public RDFIndexEntity() { }
        public RDFIndexEntity(string id) : base(id){}
        public RDFIndexEntity(string id, string label) : base(id, label){}
        public RDFIndexEntity(ISubject baseSubject) : base (baseSubject) { }

        // IHasIdProperties
        public IList<string> Properties { get; set; } = new List<string>();

        // IHasRank
        public double Rank { get; set; }
    }
}
