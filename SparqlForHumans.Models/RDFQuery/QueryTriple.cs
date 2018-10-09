using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Models.RDFQuery
{
    public class QueryTriple : IQueryTriple
    {
        public IQueriableSubject Subject { get; set; }
        public IQueriableSubject Predicate { get; set; }
        public ILabel Object { get; set; }
    }
}
