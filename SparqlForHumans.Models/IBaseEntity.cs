using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IBaseEntity : ISubject
    {
        string Description { get; set; }
        IList<string> InstanceOf { get; set; }
        IList<string> AltLabels { get; set; }
        bool IsType { get; set; }
    }
}