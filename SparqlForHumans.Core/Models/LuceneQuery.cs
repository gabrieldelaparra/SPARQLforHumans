using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Core.Models
{
    public class LuceneQuery
    {
        public string MyProperty { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Labels { get; set; } = new List<string>();
        public string Label
        {
            get
            {
                if (Labels.Count().Equals(0))
                    return string.Empty;

                return Labels.FirstOrDefault();
            }
        }
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
