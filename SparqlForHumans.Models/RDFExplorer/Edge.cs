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
        public int sourceId { get; set; }
        public int targetId { get; set; }
        public string[] uris { get; set; } = new string[0];

        public override bool Equals(object obj)
        {
            var y = obj as Edge;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return id.Equals(y.id) && sourceId.Equals(y.sourceId) && targetId.Equals(y.targetId) &&
                   uris.SequenceEqual(y.uris);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() ^ sourceId.GetHashCode() ^ targetId.GetHashCode() ^ uris.GetHashCode();
        }

        public override string ToString()
        {
            return $"{id}:{name} [{sourceId}->{targetId}] ({string.Join(";", uris.Select(x => x.GetUriIdentifier()))})";
        }
    }
}