using System.Linq;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Models.RDFExplorer
{
    public class Node
    {
        public Node() { }

        public Node(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public Node(int id, string name, string[] uris)
        {
            this.id = id;
            this.name = name;
            this.uris = uris;
        }

        public int id { get; set; }

        public string name { get; set; }

        public string[] uris { get; set; } = new string[0];

        public override bool Equals(object obj)
        {
            var y = obj as Node;
            if (y == null) return false;
            if (ReferenceEquals(this, y)) return true;
            if (ReferenceEquals(this, null) || ReferenceEquals(y, null)) return false;
            return this.id.Equals(y.id) && this.uris.SequenceEqual(y.uris);
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode() ^ this.uris.GetHashCode();
        }

        public override string ToString()
        {
            return $"{id}:{name} {(uris.Any() ? string.Join(";", uris.Select(x => x.GetUriIdentifier())) : string.Empty)}";
        }
    }
}
