using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Property : Subject, IProperty
    {
        public string Value { get; set; } = string.Empty;
        public int Rank { get; set; } = 0;
        public IList<int> Domain { get; set; } = new List<int>();
        public IList<string> AltLabels { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public IList<int> Range { get; set; } = new List<int>();

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