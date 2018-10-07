using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Property : Subject, IProperty
    {
        public Property()
        {
        }

        public Property(ISubject baseSubject)
        {
            Id = baseSubject.Id;
            Label = baseSubject.Label;
        }

        public string Value { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;

        public int FrequencyValue => int.TryParse(Frequency, out var value) ? value : 0;

        public IEnumerable<string> DomainTypes { get; set; } = new List<string>();

        public string ToRankedString()
        {
            return $"[{FrequencyValue}] {ToString()}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} -> {Value}";
        }
    }
}