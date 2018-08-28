using System;

namespace SparqlForHumans.Core.Models
{
    public class Property
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Frequency { get; set; } = 0;

        public override string ToString()
        {
            return $"[{Frequency}]{Id} {Label} -> {Value}";
        }
    }
}