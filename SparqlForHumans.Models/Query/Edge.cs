using System.Linq;

namespace SparqlForHumans.Models.Query
{
    public class Edge
    {
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
    }
}
