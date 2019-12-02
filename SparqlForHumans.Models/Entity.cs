using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Entity : Subject, IEntity
    {
        //public IList<string> SubClass { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public IList<string> InstanceOf { get; set; } = new List<string>();
        //public IList<string> ReverseInstanceOf { get; set; } = new List<string>();
        public IList<string> AltLabels { get; set; } = new List<string>();
        public bool IsType { get; set; } = false;
        public double Rank { get; set; } = 0.0;
        public IList<Property> Properties { get; set; } = new List<Property>();
        public IList<Property> ReverseProperties { get; set; } = new List<Property>();

        public override string ToString()
        {
            return $"{base.ToString()} - ({string.Join("-", InstanceOf)}) - {Description}";
        }

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }
    }
    public class EntityIdEqualityComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y)
        {
            if(null==x || null==y) return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Entity obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}