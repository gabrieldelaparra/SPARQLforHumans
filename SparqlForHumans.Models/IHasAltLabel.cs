using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IHasAltLabel
    {
        IList<string> AltLabels { get; set; }
    }

    public interface IHasInstanceOf
    {
        IList<string> InstanceOf { get; set; }
    }
    public interface IHasIsType
    {
        bool IsType { get; set; }
    }
}