using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFEntity : Entity, IQueriableSubject
    {
        public RDFEntity(){}

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
