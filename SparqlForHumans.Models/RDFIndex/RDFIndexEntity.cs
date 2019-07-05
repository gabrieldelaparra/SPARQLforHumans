using System.Collections.Generic;

namespace SparqlForHumans.Models.RDFIndex
{
    public class RDFIndexEntity : BaseEntity, IHasProperties<string>, IHasRank<double>
    {
        // IHasIdProperties
        public IList<string> Properties { get; set; } = new List<string>();

        // IHasRank
        public double Rank { get; set; }
    }
}