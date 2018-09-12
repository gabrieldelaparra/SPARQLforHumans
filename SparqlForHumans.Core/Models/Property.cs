using System;

namespace SparqlForHumans.Core.Models
{
    public class Property : BaseSubject
    {
        public string Value { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;

        public int FrequencyValue => int.TryParse(Frequency, out var value) ? value : 0;

        public Property()
        {
            
        }

        public Property(IEntity baseSubject)
        {
            Id = baseSubject.Id;
            Label = baseSubject.Label;
        }

        public override string ToString()
        {
            return $"[{Frequency}]{base.ToString()} -> {Value}";
        }
    }
}