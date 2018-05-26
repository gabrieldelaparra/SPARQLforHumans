using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.Core.Utilities
{
    public static class RDFExtensions
    {
        public static string[] ValidLanguages { get; } = { "en", };

        public static Triple GetTriple(this string inputLine)
        {
            var g = new NonIndexedGraph();
            StringParser.Parse(g, inputLine);
            return g.Triples.Last();
        }       

        public static bool IsUriNode(this INode node)
        {
            return node.NodeType.Equals(NodeType.Uri);
        }

        public static bool IsLiteral(this INode node)
        {
            return node.NodeType.Equals(NodeType.Literal);
        }

        public static bool IsValidLanguage(string literalLanguage)
        {
            return ValidLanguages.Any(x => x.Equals(literalLanguage));
        }

        public static bool IsValidLanguageLiteral(this INode node)
        {
            if (!node.IsLiteral()) return false;
            return IsValidLanguage(((LiteralNode)node).Language);
        }

        public static string GetUri(this INode node)
        {
            if (!node.IsUriNode()) return string.Empty;
            return ((UriNode)node).Uri.ToSafeString();
        }

        public static bool IsValidSubject(this INode subject)
        {
            return subject.IsEntity() || subject.IsProperty();
        }

        public static bool IsEntity(this INode node)
        {
            if (!node.IsUriNode()) return false;
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
