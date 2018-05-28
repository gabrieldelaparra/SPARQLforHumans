using System.Collections.Generic;

namespace SparqlForHumans.Core.Models
{
    public class Entity
    {
        public string Id { get; set; }

        public string Label { get; set; }
        public string Description { get; set; }
        public string InstanceOf { get; set; }

        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<string> AltLabels { get; set; } = new List<string>();

        public string InstanceOfLabel { get; set; }

        public override string ToString()
        {
            return $"{Label} ({Id}) - {InstanceOfLabel} ({InstanceOf}) - {Description}";
        }
    }
}