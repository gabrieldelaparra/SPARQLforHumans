using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IProperty : ISubject, IHasRank<int>, IHasAltLabel, IHasDescription
    {
        string Value { get; set; }
        IEnumerable<string> DomainTypes { get; set; }
    }
}