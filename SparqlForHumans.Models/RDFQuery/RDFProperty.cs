using System;
using System.Collections.Generic;
using System.Text;
using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFProperty : Property, IQueriableSubject
    {
        public string Uri()
        {
            return this.WikidataUri();
        }
    }
}
