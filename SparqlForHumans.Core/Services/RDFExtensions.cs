using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.Core.Services
{
    public static class RDFExtensions
    {
        public static Triple GetTriple(this string inputLine)
        {
            var g = new NonIndexedGraph();
            StringParser.Parse(g, inputLine);
            return g.Triples.Last();
        }

        public static bool IsUriNode(this INode node)
        {
            return node.NodeType == NodeType.Uri;
        }

        public static bool IsValidSubject(this INode node)
        {
            return node.ToSafeString().Contains(Properties.WikidataDump.EntityIRI)
                || node.ToSafeString().StartsWith(Properties.WikidataDump.PropertyIRI);
        }

        public static bool IsEntity(this INode node)
        {
            if (node.GetType() != typeof(UriNode)) return false;
            return ((UriNode)node).Uri.ToSafeString().Contains(Properties.WikidataDump.EntityIRI);
        }

        public static bool IsProperty(this INode node)
        {
            if (node.GetType() != typeof(UriNode)) return false;
            return ((UriNode)node).Uri.ToSafeString().Contains(Properties.WikidataDump.PropertyIRI);
        }

        private static string GetQ(this INode node)
        {
            return ((UriNode)node).Uri.Segments.Last();
        }

        private static string GetQCode(this INode node)
        {
            return node.GetQ().Replace(Properties.WikidataDump.EntityPrefix, string.Empty);
        }

        public static int EntityQCode(this INode node)
        {
            var index = node.GetQCode();
            var parsed = int.TryParse(index, out int indexInt);
            return parsed ? indexInt : 0;
        }
    }
}
