using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Models
{
    public class Entity : BaseEntity, IHasRank<double>, IHasProperties<Property>
    {
        //Constructor
        public Entity() : base() { }
        public Entity(string id) : base(id) { }
        public Entity(string id, string label) : base(id, label) { }
        public Entity(ISubject baseSubject) : base (baseSubject) { }

        public IList<Property> Properties { get; set; } = new List<Property>();

        public double Rank { get; set; } = 0.0;

        public string ToRankedString() => $"[{Rank}] {ToString()}";

    }
}