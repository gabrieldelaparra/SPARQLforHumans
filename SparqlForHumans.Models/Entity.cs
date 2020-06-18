using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Entity : Subject, IEntity
    {
        public IList<Property> ReverseProperties { get; set; } = new List<Property>();
        public IList<string> SubClass { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public IList<string> ParentTypes { get; set; } = new List<string>();
        public IList<string> AltLabels { get; set; } = new List<string>();
        public bool IsType { get; set; } = false;
        public double Rank { get; set; } = 0.0;
        //public IList<string> ReverseInstanceOf { get; set; } = new List<string>();
        public IList<Property> Properties { get; set; } = new List<Property>();

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} - ({string.Join("-", ParentTypes)}) - {Description}";
        }
    }
}