using System.Linq;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.RDF.Extensions
{
    public static class RDFExtensions
    {
        public static string[] ValidLanguages { get; } = {"en"};

        public static PropertyType GetPropertyType(INode ntPredicate)
        {
            if (ntPredicate.IsInstanceOf()) return PropertyType.InstanceOf;

            if (ntPredicate.IsSubClass()) return PropertyType.SubClass;
            return PropertyType.Other;
        }

        public static (INode subject, INode predicate, INode ntObject) AsTuple(this Triple triple)
        {
            return (triple.Subject, triple.Predicate, triple.Object);
        }

        public static (INode subject, INode predicate, INode ntObject) GetTripleAsTuple(this string inputLine)
        {
            var triple = inputLine.ToTriple();
            return (triple.Subject, triple.Predicate, triple.Object);
        }

        public static Triple ToTriple(this string inputLine)
        {
            var g = new NonIndexedGraph();
            StringParser.Parse(g, inputLine);
            return g.Triples?.Last();
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
            return node.IsLiteral() ? ((LiteralNode) node).Value : string.Empty;
        }

        public static string GetUri(this INode node)
        {
            return node.IsUriNode() ? ((UriNode) node).Uri.ToString() : string.Empty;
        }

        public static bool IsEntity(this INode node)
        {
            return node.IsUriNode() && node.GetUri().StartsWith(WikidataDump.EntityIRI);
        }

        public static bool IsEntityQ(this INode node)
        {
            return node.IsEntity() && node.GetId().StartsWith(WikidataDump.EntityPrefix);
        }

        public static bool IsEntityP(this INode node)
        {
            return node.IsEntity() && node.GetId().StartsWith(WikidataDump.PropertyPrefix);
        }

        public static bool IsProperty(this INode node)
        {
            return node.IsUriNode() && node.GetUri().StartsWith(WikidataDump.PropertyIRI);
        }

        public static PredicateType GetPredicateType(this INode node)
        {
            if (node.IsLabel())
                return PredicateType.Label;

            if (node.IsDescription())
                return PredicateType.Description;

            if (node.IsAltLabel())
                return PredicateType.AltLabel;

            if (node.IsProperty())
                return PredicateType.Property;

            return PredicateType.Other;
        }

        private static bool IsLabel(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.LabelIRI);
        }

        public static bool IsInstanceOf(this INode node)
        {
            return node.GetId().Equals(WikidataDump.InstanceOf);
        }

        public static bool IsSubClass(this INode node)
        {
            return node.GetId().Equals(WikidataDump.SubClass);
        }

        private static bool IsDescription(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.DescriptionIRI);
        }

        private static bool IsAltLabel(this INode node)
        {
            return node.GetUri().Equals(WikidataDump.Alt_labelIRI);
        }

        public static int GetIntId(this INode node)
        {
            return node.GetId().ToNumbers();
        }

        public static string GetId(this INode node)
        {
            return ((UriNode) node).Uri.Segments.Last();
        }
    }
}