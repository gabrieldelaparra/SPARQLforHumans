using SparqlForHumans.Models.Extensions;

namespace SparqlForHumans.Models.RDFQuery
{
    public class RDFEntity : Entity, IQueriableSubject
    {
        public string Uri()
        {
            return this.WikidataUri();
        }
    }
}
