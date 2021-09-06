using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class RDFExtensionsTests
    {
        [Fact]
        public void TestFailGetTriple()
        {
            var line = "hola";
            Assert.Throws<RdfParseException>(() => line.ToTriple());
        }

        [Fact]
        public void TestGetId()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/P26> .";
            var triple = line.ToTriple();

            Assert.Equal("Q27", triple.Subject.GetId());
            Assert.Equal("P47", triple.Predicate.GetId());
            Assert.Equal("P26", triple.Object.GetId());
        }

        [Fact]
        public void TestGetLiteral()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.Empty(triple.Subject.GetLiteralValue());
            Assert.Empty(triple.Predicate.GetLiteralValue());
            Assert.NotEmpty(triple.Object.GetLiteralValue());
            Assert.Equal("Ireland", triple.Object.GetLiteralValue());
        }

        [Fact]
        public void TestGetPredicateType()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();
            var predicate = triple.Predicate.GetPredicateType();
            Assert.Equal(PredicateType.Label, predicate);
            Assert.NotEqual(PredicateType.AltLabel, predicate);
            Assert.NotEqual(PredicateType.Description, predicate);
            Assert.NotEqual(PredicateType.Other, predicate);
            Assert.NotEqual(PredicateType.Property, predicate);

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2004/02/skos/core#altLabel> \"Ireland\"@en .";
            triple = line.ToTriple();
            predicate = triple.Predicate.GetPredicateType();
            Assert.NotEqual(PredicateType.Label, predicate);
            Assert.Equal(PredicateType.AltLabel, predicate);
            Assert.NotEqual(PredicateType.Description, predicate);
            Assert.NotEqual(PredicateType.Other, predicate);
            Assert.NotEqual(PredicateType.Property, predicate);

            line = "<http://www.wikidata.org/entity/Q27> <http://schema.org/description> \"Ireland\"@en .";
            triple = line.ToTriple();
            predicate = triple.Predicate.GetPredicateType();
            Assert.NotEqual(PredicateType.Label, predicate);
            Assert.NotEqual(PredicateType.AltLabel, predicate);
            Assert.Equal(PredicateType.Description, predicate);
            Assert.NotEqual(PredicateType.Other, predicate);
            Assert.NotEqual(PredicateType.Property, predicate);

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2004/02/skos/core#prefLabel> \"Ireland\"@en .";
            triple = line.ToTriple();
            predicate = triple.Predicate.GetPredicateType();
            Assert.NotEqual(PredicateType.Label, predicate);
            Assert.NotEqual(PredicateType.AltLabel, predicate);
            Assert.NotEqual(PredicateType.Description, predicate);
            Assert.Equal(PredicateType.Other, predicate);
            Assert.NotEqual(PredicateType.Property, predicate);

            line = "<http://www.wikidata.org/entity/Q27> <http://schema.org/name> \"Ireland\"@en .";
            triple = line.ToTriple();
            predicate = triple.Predicate.GetPredicateType();
            Assert.NotEqual(PredicateType.Label, predicate);
            Assert.NotEqual(PredicateType.AltLabel, predicate);
            Assert.NotEqual(PredicateType.Description, predicate);
            Assert.Equal(PredicateType.Other, predicate);
            Assert.NotEqual(PredicateType.Property, predicate);

            line = "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P1559> \"Ireland\"@en .";
            triple = line.ToTriple();
            predicate = triple.Predicate.GetPredicateType();
            Assert.NotEqual(PredicateType.Label, predicate);
            Assert.NotEqual(PredicateType.AltLabel, predicate);
            Assert.NotEqual(PredicateType.Description, predicate);
            Assert.NotEqual(PredicateType.Other, predicate);
            Assert.Equal(PredicateType.Property, predicate);
        }

        [Fact]
        public void TestGetQCode()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/P26> .";
            var triple = line.ToTriple();

            Assert.Equal(27, triple.Subject.GetIntId());
            Assert.Equal(47, triple.Predicate.GetIntId());
            Assert.Equal(26, triple.Object.GetIntId());
        }

        [Fact]
        public void TestGetTriple()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.NotNull(triple);
            Assert.IsType<Triple>(triple);
        }

        [Fact]
        public void TestGetUri()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.Equal("http://www.wikidata.org/entity/Q27", triple.Subject.GetUri());
            Assert.Equal("http://www.w3.org/2000/01/rdf-schema#label", triple.Predicate.GetUri());
            Assert.Empty(triple.Object.GetUri());
        }

        [Fact]
        public void TestGetURIequalsNodeToString()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.Equal(triple.Subject.ToString(), triple.Subject.GetUri());
            Assert.Equal(triple.Predicate.ToString(), triple.Predicate.GetUri());
            Assert.Empty(triple.Object.GetUri());
        }

        [Fact]
        public void TestIsEntity()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.True(triple.Subject.IsEntity());
            Assert.False(triple.Predicate.IsEntity());
            Assert.False(triple.Object.IsEntity());

            line =
                "<http://www.wikidata.org/entity/P31> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();

            Assert.True(triple.Subject.IsEntity());
            Assert.False(triple.Predicate.IsEntity());
            Assert.True(triple.Object.IsEntity());

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();

            Assert.True(triple.Subject.IsEntity());

            line =
                "<http://www.wikidata.org/entity/P27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();

            Assert.True(triple.Subject.IsEntity());

            line =
                "<http://www.wikidata.org/NotEntity/Q666> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();

            Assert.False(triple.Subject.IsEntity());

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();
            Assert.True(triple.Subject.IsEntity());
        }

        [Fact]
        public void TestIsInstanceOf()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            var triple = line.ToTriple();

            Assert.False(triple.Predicate.IsInstanceOf());

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();

            Assert.True(triple.Predicate.IsInstanceOf());
        }

        [Fact]
        public void TestIsLiteral()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.False(triple.Subject.IsLiteral());
            Assert.False(triple.Predicate.IsLiteral());
            Assert.True(triple.Object.IsLiteral());
        }

        [Fact]
        public void TestIsProperty()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.False(triple.Subject.IsProperty());
            Assert.False(triple.Predicate.IsProperty());
            Assert.False(triple.Object.IsProperty());

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();

            Assert.False(triple.Subject.IsProperty());
            Assert.True(triple.Predicate.IsProperty());
            Assert.False(triple.Object.IsProperty());
        }

        [Fact]
        public void TestReorderProperty()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            var triple = line.ToTriple();
            var reordered = triple.ReorderTriple();

        }

        [Fact]
        public void TestIsSubClass()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            var triple = line.ToTriple();

            Assert.False(triple.Predicate.IsSubClass());

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P279> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();

            Assert.True(triple.Predicate.IsSubClass());
        }

        [Fact]
        public void TestIsUriNode()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.True(triple.Subject.IsUriNode());
            Assert.True(triple.Predicate.IsUriNode());
            Assert.False(triple.Object.IsUriNode());
        }

        [Fact]
        public void TestIsValidLanguage()
        {
            string[] validLanguages = { "en", "es" };
            Assert.True(RDFExtensions.IsValidLanguage("en", validLanguages));
            Assert.True(RDFExtensions.IsValidLanguage("es", validLanguages));
            Assert.False(RDFExtensions.IsValidLanguage("de", validLanguages));
        }

        [Fact]
        public void TestIsValidLanguageDefault()
        {
            Assert.True(RDFExtensions.IsValidLanguage("en"));
            Assert.False(RDFExtensions.IsValidLanguage("de"));
        }

        [Fact]
        public void TestIsValidLanguageLiteral()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();

            Assert.False(triple.Subject.IsValidLanguageLiteral());
            Assert.False(triple.Predicate.IsValidLanguageLiteral());
            Assert.True(triple.Object.IsValidLanguageLiteral());

            line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@de .";
            triple = line.ToTriple();

            Assert.False(triple.Subject.IsValidLanguageLiteral());
            Assert.False(triple.Predicate.IsValidLanguageLiteral());
            Assert.False(triple.Object.IsValidLanguageLiteral());
        }

        [Fact]
        public void TestPropertyType()
        {
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P417> \"Ireland\"@en .";
            var (_, ntPredicate, ntObject) = line.GetTripleAsTuple();

            Assert.True(ntPredicate.IsProperty());

            Assert.Equal(PropertyType.Other,
                RDFExtensions.GetPropertyType(ntPredicate));

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            (_, ntPredicate, ntObject) = line.GetTripleAsTuple();
            Assert.Equal(PropertyType.Other,
                RDFExtensions.GetPropertyType(ntPredicate));

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P31> <http://www.wikidata.org/entity/Q26> .";
            (_, ntPredicate, ntObject) = line.GetTripleAsTuple();
            Assert.Equal(PropertyType.InstanceOf, RDFExtensions.GetPropertyType(ntPredicate));

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/Other/Q32> <http://www.wikidata.org/other/P26> .";
            (_, ntPredicate, ntObject) = line.GetTripleAsTuple();
            Assert.Equal(PropertyType.Other, RDFExtensions.GetPropertyType(ntPredicate));
        }

        [Fact]
        public void TestEmptyLinesToSubjectGroup()
        {
            var empty = string.Empty;
            var triple = empty.ToTriple();
            Assert.Null(triple);

            var elements = new List<string>() {empty};
            var triples = elements.Select(x => x.ToTriple());

        }
    }
}