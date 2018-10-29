using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFGraph : IRDFGraph
    {
        public List<RDFTriple> QueryTriples { get; set; } = new List<RDFTriple>();
    }
}
