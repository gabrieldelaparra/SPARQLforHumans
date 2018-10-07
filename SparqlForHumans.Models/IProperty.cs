using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IProperty
    {
        string Value { get; set; }
        string Frequency { get; set; }
        int FrequencyValue { get; }
        IEnumerable<string> DomainTypes { get; set; }
    }
}
