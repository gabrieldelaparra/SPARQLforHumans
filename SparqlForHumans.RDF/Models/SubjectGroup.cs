using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace SparqlForHumans.RDF.Models
{
    public class SubjectGroup : Grouping<string, Triple>
    {
        public SubjectGroup() : this(string.Empty, new List<Triple>())
        {
        }

        public SubjectGroup(string key, IEnumerable<string> elements) : this(key, elements.Select(x => x.ToTriple()).Where(x=>x != null))
        {
        }

        public SubjectGroup(string key, IEnumerable<Triple> elements) : base(key, elements)
        {
            Id = key.Replace($"<{Constants.EntityIRI}", string.Empty).Replace(">", string.Empty);
            IntId = Id.ToInt();
        }

        public string Id { get; }
        public int IntId { get; }
    }
}