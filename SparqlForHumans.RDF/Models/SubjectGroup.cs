using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.RDF.Models
{
    public class SubjectGroup : Grouping<string, Triple>
    {
        public SubjectGroup() : this(string.Empty, new List<Triple>())
        {
        }

        public SubjectGroup(string key, IEnumerable<string> elements) : this(key, elements.Select(x => x.ToTriple()))
        {
        }

        public SubjectGroup(string key, IEnumerable<Triple> elements) : base(key, elements)
        {
            Id = key.Replace($"<{WikidataDump.EntityIRI}", string.Empty).Replace(">", string.Empty);
            IntId = Id.ToNumbers();
        }

        public string Id { get; }
        public int IntId { get; }
    }
}