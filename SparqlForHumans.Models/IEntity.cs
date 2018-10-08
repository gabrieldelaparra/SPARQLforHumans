using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public interface IEntity : ISubject
    {
        string Description { get; set; }

        IEnumerable<string> InstanceOf { get; set; }

        string InstanceOfId { get; }

        string InstanceOfLabel { get; }

        IEnumerable<IProperty> Properties { get; set; }

        IEnumerable<string> AltLabels { get; set; }

        string Rank { get; set; }

        double RankValue { get; }
    }
}