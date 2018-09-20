using System;
using System.Collections.Generic;

namespace SparqlForHumans.Core.Models
{
    public class Property : BaseSubject
    {
        public string Value { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;

        public int FrequencyValue => int.TryParse(Frequency, out var value) ? value : 0;

        public IEnumerable<string> DomainTypes { get; set; } = new List<string>();

        public Property()
        {
            
        }

        public Property(IEntity baseSubject)
        {
            Id = baseSubject.Id;
            Label = baseSubject.Label;
        }

        public string ToRankedString()
        {
            return $"[{Frequency}] {ToString()}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} -> {Value}";
        }
    }
}