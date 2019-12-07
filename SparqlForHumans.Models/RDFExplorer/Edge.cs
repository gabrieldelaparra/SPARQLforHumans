using System.Linq;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Models.RDFExplorer
{
    public class Edge
    {
        public Edge() { }

        public Edge(int id, string name, int sourceId, int targetId)
        {
            this.id = id;
            this.name = name;
            this.sourceId = sourceId;
            this.targetId = targetId;
        }

        public Edge(int id, string name, int sourceId, int targetId, string[] uris)
        {
            this.id = id;
            this.name = name;
            this.sourceId = sourceId;
            this.targetId = targetId;
            this.uris = uris;
        }

        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public int sourceId { get; set; }

        public int targetId { get; set; }

        public override bool Equals(object obj)
        {
            var y = obj as Edge;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return this.id.Equals(y.id) && this.sourceId.Equals(y.sourceId) && this.targetId.Equals(y.targetId) && this.uris.SequenceEqual(y.uris);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode() ^ this.sourceId.GetHashCode() ^ this.targetId.GetHashCode() ^ this.uris.GetHashCode();
        }
        public override string ToString()
        {
            return $"{id}:{name} - [{sourceId}->{targetId}] {(uris.Any() ? string.Join(";", uris.Select(x => x.GetUriIdentifier())) : string.Empty)}";
        }

    }
}
