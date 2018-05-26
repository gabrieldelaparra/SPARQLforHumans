using SparqlForHumans.Core.Services;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class RDFExtensionsTests
    {
        [Fact]
        public void TestGetTriple()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.NotNull(triple);
            Assert.IsType<Triple>(triple);
        }

        [Fact]
        public void TestFailGetTriple()
        {
            var line = "hola";
            Assert.Throws<RdfParseException>(() => line.GetTriple());
        }

        [Fact]
        public void TestIsUriNode()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.True(triple.Subject.IsUriNode());
            Assert.True(triple.Predicate.IsUriNode());
            Assert.False(triple.Object.IsUriNode());
        }

        [Fact]
        public void TestIsEntity()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.True(triple.Subject.IsEntity());
            Assert.False(triple.Predicate.IsEntity());
            Assert.False(triple.Object.IsEntity());

            line = "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.GetTriple();

            Assert.True(triple.Subject.IsEntity());
            Assert.False(triple.Predicate.IsEntity());
            Assert.True(triple.Object.IsEntity());
        }

        [Fact]
        public void TestIsProperty()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.False(triple.Subject.IsProperty());
            Assert.False(triple.Predicate.IsProperty());
            Assert.False(triple.Object.IsProperty());

            line = "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.GetTriple();

            Assert.False(triple.Subject.IsProperty());
            Assert.True(triple.Predicate.IsProperty());
            Assert.False(triple.Object.IsProperty());
        }

        [Fact]
        public void TestIsValidSubjectProperty()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();
            var isValid = triple.Subject.IsValidSubject();
            Assert.True(isValid);
        }

        [Fact]
        public void TestIsLiteral()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.False(triple.Subject.IsLiteral());
            Assert.False(triple.Predicate.IsLiteral());
            Assert.True(triple.Object.IsLiteral());
        }

        [Fact]
        public void TestIsValidLanguageLiteral()
        {
            var line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.GetTriple();

            Assert.False(triple.Subject.IsValidLanguageLiteral());
            Assert.False(triple.Predicate.IsValidLanguageLiteral());
            Assert.True(triple.Object.IsValidLanguageLiteral());

            line = "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@de .";
            triple = line.GetTriple();

            Assert.False(triple.Subject.IsValidLanguageLiteral());
            Assert.False(triple.Predicate.IsValidLanguageLiteral());
            Assert.False(triple.Object.IsValidLanguageLiteral());

        }
    }
}
