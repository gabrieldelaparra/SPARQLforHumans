using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Entity : BaseEntity, IHasRank<double>, IHasProperties<Property>
    {
        //Constructor
        public Entity()
        {
        }

        public Entity(string id) : base(id)
        {
        }

        public Entity(string id, string label) : base(id, label)
        {
        }

        public Entity(ISubject baseSubject) : base(baseSubject)
        {
        }

        public IList<Property> Properties { get; set; } = new List<Property>();

        public double Rank { get; set; } = 0.0;

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }
    }
}