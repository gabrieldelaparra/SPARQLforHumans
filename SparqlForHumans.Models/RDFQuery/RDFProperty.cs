using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFProperty : Property, IRDFSubject
    {
        public RDFProperty()
        {
        }

        public RDFProperty(Property property)
        {
            Id = property.Id;
            Label = property.Label;
        }

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