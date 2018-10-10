using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFProperty : Property, IQueriableSubject
    {
        public RDFProperty() {}

        public RDFProperty(ISubject iSubject)
        {
            Id = iSubject.Id;
        }

        public string Uri()
        {
            return this.WikidataUri();
        }
    }
}
