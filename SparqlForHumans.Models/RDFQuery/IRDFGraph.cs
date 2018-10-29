using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models.RDFQuery
{
    interface IRDFGraph
    {
        List<RDFTriple> QueryTriples { get; set; }
    }
}
