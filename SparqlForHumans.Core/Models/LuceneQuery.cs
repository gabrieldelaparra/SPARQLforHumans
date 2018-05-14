using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Core.Models
{
    public class LuceneQuery
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string TypeLabel { get; set; }
    }
}
