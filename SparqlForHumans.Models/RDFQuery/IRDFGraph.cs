using System.Collections.Generic;

namespace SparqlForHumans.Models.RDFQuery
{
    internal interface IRDFGraph
    {
        List<RDFTriple> QueryTriples { get; set; }
    }
}