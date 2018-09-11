using System;
using System.Linq;
using System.Text.RegularExpressions;
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

        public enum PropertyType
        {
            InstanceOf,
            EntityDirected,
            LiteralDirected,
            Other,
        }

        public static PropertyType GetPropertyType(INode ntPredicate, INode ntObject)
        {
            if (ntPredicate.IsInstanceOf())
                return PropertyType.InstanceOf;
            if (ntObject.IsEntity())
                return PropertyType.EntityDirected;
            if (ntObject.IsLiteral())
                return PropertyType.LiteralDirected;
            return PropertyType.Other;
        }

        public static string[] ValidLanguages { get; } = { "en" };

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

        ////TODO: Test
        //public static string GetTripleId(this string inputLine)
        //{
        //    return inputLine.Split(" ")
        //        .FirstOrDefault()
        //        .Replace(WikidataDump.EntityIRI, 
        //            string.Empty,
        //            StringComparison.CurrentCultureIgnoreCase);
        //}

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
            return node.IsLiteral() && IsValidLanguage(((LiteralNode)node).Language);
        }

        public static string GetLiteralValue(this INode node)
        {
            return node.IsLiteral() ? ((LiteralNode)node).Value : string.Empty;
        }

        public static string GetUri(this INode node)
        {
            return node.IsUriNode() ? ((UriNode)node).Uri.ToSafeString() : string.Empty;
        }

        public static bool IsEntity(this INode node)
        {
            return node.IsUriNode() && node.GetUri().Contains(WikidataDump.EntityIRI);
        }

        public static bool IsEntityQ(this INode node)
        {
            return node.IsEntity() && node.GetId().Contains(WikidataDump.EntityPrefix);
        }

        public static bool IsEntityP(this INode node)
        {
            return node.IsEntity() && node.GetId().Contains(WikidataDump.PropertyPrefix);
        }

        public static bool IsProperty(this INode node)
        {
            return node.IsUriNode() && node.GetUri().Contains(WikidataDump.PropertyIRI);
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
            var index = Regex.Replace(node.GetId(), "\\D", string.Empty);
            return int.Parse(index);
        }

        public static string GetId(this INode node)
        {
            return ((UriNode)node).Uri.Segments.Last();
        }
    }
}