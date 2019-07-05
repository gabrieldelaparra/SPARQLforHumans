using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Property : Subject, IProperty
    {
        public string Value { get; set; } = string.Empty;
        public int Rank { get; set; } = 0;
        public IEnumerable<string> DomainTypes { get; set; } = new List<string>();
        public IList<string> AltLabels { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }

        public override string ToString()
        {
            var basic = $"{base.ToString()}";
            return string.IsNullOrWhiteSpace(Value) ? basic : $"{basic} -> {Value}";
        }
    }
}