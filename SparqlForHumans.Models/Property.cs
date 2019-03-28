using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Property : Subject, IProperty
    {
        public Property() : base() { }
        public Property(string id) : base(id) { }
        public Property(string id, string label) : base(id, label) { }
        public Property(ISubject baseSubject) : base (baseSubject) { }

        public string Value { get; set; } = string.Empty;
        public int Rank { get; set; } = 0;

        public IEnumerable<string> DomainTypes { get; set; } = new List<string>();

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} -> {Value}";
        }
    }
}