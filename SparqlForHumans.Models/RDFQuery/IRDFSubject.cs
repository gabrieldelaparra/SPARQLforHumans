using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models.RDFQuery
{
    public interface IRDFSubject : ISubject
    {
        string Uri();
    }
}
