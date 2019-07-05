using SparqlForHumans.Models.Extensions;
using System.Collections.Generic;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFEntity : BaseEntity, IHasProperties<Property>, IHasRank<double>, IHasURI
    {
        public RDFEntity()
        {
        }

        public RDFEntity(Entity entity)
        {
            Id = entity.Id;
            Label = entity.Label;
            Description = entity.Description;
            AltLabels = entity.AltLabels;
            InstanceOf = entity.InstanceOf;
            Properties = entity.Properties;
        }

        public RDFEntity(IHasId hasId)
        {
            Id = hasId.Id;
        }

        public IList<Property> Properties { get; set; }

        public double Rank { get; set; }

        public string Uri()
        {
            return this.WikidataUri();
        }
    }
}