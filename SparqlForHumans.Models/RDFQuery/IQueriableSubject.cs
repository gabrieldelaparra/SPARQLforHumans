using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models.RDFQuery
{
    public interface IQueriableSubject : ISubject
    {
        string Uri();
    }
}
