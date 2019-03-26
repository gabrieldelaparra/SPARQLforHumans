using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFEntity : Entity, IRDFSubject
    {
        public RDFEntity()
        {
        }

        public RDFEntity(IEntity entity)
        {
            Id = entity.Id;
            Label = entity.Label;
            Description = entity.Description;
            AltLabels = entity.AltLabels;
            InstanceOf = entity.InstanceOf;
            Properties = entity.Properties;
        }

        public RDFEntity(ISubject iSubject)
        {
            Id = iSubject.Id;
        }

        public string Uri()
        {
            return this.WikidataUri();
        }
    }
}