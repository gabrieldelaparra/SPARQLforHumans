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

        //TODO: Test
        public static bool IsValidLanguage(string literalLanguage)
        {
            return ValidLanguages.Any(x => x.Equals(literalLanguage));
        }

        public static bool IsValidLanguageLiteral(this INode node)
        {
            if (!node.IsLiteral()) return false;
            return IsValidLanguage(((LiteralNode)node).Language);
        }

        //TODO: Test
        public static string GetLiteralValue(this INode node)
        {
            if (!node.IsLiteral()) return string.Empty;
            return ((LiteralNode)node).Value;
        }

        //TODO: Test
        public static string GetUri(this INode node)
        {
            if (!node.IsUriNode()) return string.Empty;
            return ((UriNode)node).Uri.ToSafeString();
        }

        //TODO: Test
        public static bool IsValidSubject(this INode subject)
        {
            return subject.IsEntity() || subject.IsProperty();
        }

        public static bool IsEntity(this INode node)
        {
            if (!node.IsUriNode()) return false;
            return node.GetUri().Contains(Properties.WikidataDump.EntityIRI);
        }

        public static bool IsProperty(this INode node)
        {
            if (!node.IsUriNode()) return false;
            return node.GetUri().Contains(Properties.WikidataDump.PropertyIRI);
        }

        public enum PredicateType
        {
            Property,
            Label,
            Description,
            AltLabel,
            Other
        }        

        //TODO: Test
        public static PredicateType GetPredicateType(this INode node)
        {
            if (node.IsLabelIRI())
                return PredicateType.Label;
            else if (node.IsDescriptionIRI())
                return PredicateType.Description;
            else if (node.IsAltLabelIRI())
                return PredicateType.AltLabel;
            else if (node.IsProperty())
                return PredicateType.Property;

            return PredicateType.Other;
        }

        //TODO: Test
        public static bool IsLabelIRI(this INode node)
        {
            return node.GetUri().Equals(Properties.WikidataDump.LabelIRI);
        }

        //TODO: Test
        public static bool IsInstanceOf(this INode node)
        {
            return node.GetPCode().Equals(Properties.WikidataDump.InstanceOf);
        }

        //TODO: Test
        public static bool IsDescriptionIRI(this INode node)
        {
            return node.GetUri().Equals(Properties.WikidataDump.DescriptionIRI);
        }

        //TODO: Test
        public static bool IsAltLabelIRI(this INode node)
        {
            return node.GetUri().Equals(Properties.WikidataDump.Alt_labelIRI);
        }

        private static string GetId(this INode node)
        {
            return ((UriNode)node).Uri.Segments.Last();
        }

        public static bool HasQCode(this INode node)
        {
            return node.GetQCode().StartsWith(Properties.WikidataDump.EntityPrefix);
        }

        //TODO: Test
        public static string GetQCode(this INode node)
        {
            return node.GetId().Replace(Properties.WikidataDump.EntityPrefix, string.Empty);
        }

        //TODO: Test
        public static string GetPCode(this INode node)
        {
            return node.GetId().Replace(Properties.WikidataDump.PropertyIRI, string.Empty);
        }

        //TODO: Test
        public static int EntityQCode(this INode node)
        {
            var index = node.GetQCode();
            var parsed = int.TryParse(index, out int indexInt);
            return parsed ? indexInt : 0;
        }
    }
}
