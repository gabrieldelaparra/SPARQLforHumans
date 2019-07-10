using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IProperty : ISubject, IHasRank<int>, IHasAltLabel, IHasDescription, IHasDomain, IHasRange
    {
        string Value { get; set; }
    }
}