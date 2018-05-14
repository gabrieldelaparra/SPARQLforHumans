using System.Collections.Generic;

namespace SparqlForHumans.Core.Models
{
    public class LuceneQuery
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public IEnumerable<(string, string)> Properties { get; set; }

        public string TypeLabel { get; set; }

        public override string ToString()
        {
            return $"{Label} ({Name}) - {TypeLabel} ({Type}) - {Description}";
        }
    }
}
