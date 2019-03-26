using System.Collections.Generic;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFGraph : IRDFGraph
    {
        public List<RDFTriple> QueryTriples { get; set; } = new List<RDFTriple>();
    }
}