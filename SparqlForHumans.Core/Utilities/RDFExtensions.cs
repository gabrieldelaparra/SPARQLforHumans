using System.Linq;
using SparqlForHumans.Core.Properties;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.Core.Utilities
{
    public static class RDFExtensions
    {
        public enum PredicateType
        {
            Property,
            Label,
            Description,
            AltLabel,
            Other
        }

        public static string[] ValidLanguages { get; } = {"en"};

        public static (INode subject, INode predicate, INode ntObject) GetTripleAsTuple(this string inputLine)
        {
            var triple = inputLine.GetTriple();
            return (triple.Subject, triple.Predicate, triple.Object);
        }

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
            return IsValidLanguage(literalLanguage, ValidLanguages);
        }

        public static bool IsValidLanguage(string literalLanguage, string[] validLanguages)
        {
            return validLanguages.Any(x => x.Equals(literalLanguage));
        }

        public static bool IsValidLanguageLiteral(this INode node)
        {
            return node.IsLiteral() && IsValidLanguage(((LiteralNode) node).Language);
        }

        public static string GetLiteralValue(this INode node)
        {
            return node.IsLiteral() ? ((LiteralNode)node).Value : string.Empty ;
        }

        public static string GetUri(this INode node)
        {
            return node.IsUriNode() ? ((UriNode)node).Uri.ToSafeString() : string.Empty ;
        }

        //TODO: Test
        public static bool IsValidSubject(this INode subject)
        {
            return subject.IsEntity() || subject.IsProperty();
        }

        public static bool IsEntity(this INode node)
        {
            return node.IsUriNode() && node.GetUri().Contains(WikidataDump.EntityIRI);
        }

        public static bool IsProperty(this INode node)
        {
            return node.IsUriNode() && node.GetUri().Contains(WikidataDump.PropertyIRI);
        }

        //TODO: Test
        public static PredicateType GetPredicateType(this INode node)
        {
            if (node.IsLabelIRI())
                return PredicateType.Label;
            if (node.IsDescriptionIRI())
                return PredicateType.Description;
            if (node.IsAltLabelIRI())
                return PredicateType.AltLabel;
            if (node.IsProperty())
                return PredicateType.Property;

            return PredicateType.Other;
        }

        private static bool IsLabelIRI(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.LabelIRI);
        }

        public static bool IsInstanceOf(this INode node)
        {
            return node.GetPCode().Equals(WikidataDump.InstanceOf);
        }

        private static bool IsDescriptionIRI(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.DescriptionIRI);
        }

        private static bool IsAltLabelIRI(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.Alt_labelIRI);
        }

        public static bool HasQCode(this INode node)
        {
            return node.GetQCode().StartsWith(WikidataDump.EntityPrefix);
        }

        public static string GetQCode(this INode node)
        {
            return node.GetId();
        }

        public static string GetPCode(this INode node)
        {
            return node.GetId().Replace(WikidataDump.PropertyIRI, string.Empty);
        }

        public static int GetEntityQCode(this INode node)
        {
            var index = node.GetQCode().Replace(WikidataDump.EntityPrefix, string.Empty);
            var parsed = int.TryParse(index, out var indexInt);
            return parsed ? indexInt : 0;
        }

        private static string GetId(this INode node)
        {
            return ((UriNode) node).Uri.Segments.Last();
        }
    }
}