using System.Collections.Generic;

namespace SparqlForHumans.Models
{
    public class Entity : BaseEntity, IHasRank<double>, IHasProperties<Property>
    {
        public IList<Property> Properties { get; set; } = new List<Property>();

        public double Rank { get; set; } = 0.0;

        public string ToRankedString()
        {
            return $"[{Rank}] {ToString()}";
        }
    }
}