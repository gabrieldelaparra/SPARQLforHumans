using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Filtering;
using SparqlForHumans.Utilities;
using System.IO;
using System.Linq;
using Xunit;

namespace SparqlForHumans.UnitTests
{
    public class FilterHelperTests
    {
        [Fact]
        public void TestFilterCompareWithPlainCount()
        {
            const string filename = "Resources/Filter5k-PlainCount.nt";
            Assert.True(File.Exists(filename));
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename);

            Assert.True(File.Exists(outputFilename));
            var gZipLines = SharpZipHandler.ReadGZip(outputFilename);
            var plainLines = FileHelper.ReadLines(filename);

            Assert.Equal(gZipLines.Count(), plainLines.Count());

            outputFilename.DeleteIfExists();
        }

        [Fact]
        public void TestFilterCompareWithPlainLineByLine()
        {
            const string filename = "Resources/Filter5k.nt";
            Assert.True(File.Exists(filename));
            var outputFilename = FileHelper.GetFilteredOutputFilename(filename);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename);

            Assert.True(File.Exists(outputFilename));
            var gZipLines = SharpZipHandler.ReadGZip(outputFilename).ToArray();
            var plainLines = FileHelper.ReadLines(filename).ToArray();

            for (var i = 0; i < plainLines.Count(); i++)
            {
                Assert.Equal(gZipLines[i], plainLines[i]);
            }

            outputFilename.DeleteIfExists();
        }

        [Fact]
        public void TestFilteredOutputTypeIsCompressed()
        {
            const string filename = "Resources/FilterTrimmed.nt";
            Assert.True(File.Exists(filename));

            const int limit = 100;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, limit);

            Assert.True(File.Exists(outputFilename));
            Assert.Equal(FileHelper.FileType.gZip, FileHelper.GetFilenameType(outputFilename));

            outputFilename.DeleteIfExists();
        }

        [Fact]
        public void TestFilterFileLimit200()
        {
            const string filename = @"Resources/FilterTrimmed.nt";
            Assert.True(File.Exists(filename));

            const int limit = 200;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, limit);

            Assert.True(File.Exists(outputFilename));

            Assert.NotEqual(0, FileHelper.GetLineCount(outputFilename, 50));

            var lines = FileHelper.GetInputLines(outputFilename);
            var firstItemList = lines.Select(x => x.Split(Constants.BlankSpaceChar).FirstOrDefault());

            Assert.NotNull(firstItemList);
            Assert.NotEmpty(firstItemList);

            var qCodesList = firstItemList.Select(x => x.Split(@"<http://www.wikidata.org/entity/Q")
                .LastOrDefault()
                .Replace(">", ""));

            var intCodesList = qCodesList.Select(int.Parse);
            Assert.True(intCodesList.All(x => x < limit));
        }

        [Fact]
        public void TestFilterFileLimitZero()
        {
            const string filename = @"Resources/FilterTrimmed.nt";
            Assert.True(File.Exists(filename));

            const int limit = 0;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, 0);

            Assert.True(File.Exists(outputFilename));
            Assert.NotEqual(0, FileHelper.GetLineCount(outputFilename));

            outputFilename.DeleteIfExists();
        }

        [Fact]
        public void TestFilterGZipOutput()
        {
            const string filename = "Resources/FilterTrimmed.nt";
            Assert.True(File.Exists(filename));

            const int limit = 100;

            var outputFilename = FileHelper.GetFilteredOutputFilename(filename, limit);

            outputFilename.DeleteIfExists();

            Assert.False(File.Exists(outputFilename));

            TriplesFilter.Filter(filename, outputFilename, limit);

            Assert.True(File.Exists(outputFilename));
            var lines = SharpZipHandler.ReadGZip(outputFilename);

            Assert.NotNull(lines);
            Assert.NotEmpty(lines);

            //Hardcoded number, Heuristic.
            Assert.Equal(136, lines.Count());

            outputFilename.DeleteIfExists();
        }

        //TODO: Split into individual Tests
        [Fact]
        public void TestIsValidTriple()
        {
            //PASS: Subject: Entity-Q < 100 ; Predicate: Label ; Object: Literal
            var line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            var triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-Q < 100 ; Predicate: Description ; Object: Literal
            line =
                "<http://www.wikidata.org/entity/Q27> <http://schema.org/description> \"constitutional monarchy in Southwest Europe\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-Q < 100 ; Predicate: Alt-Label ; Object: Literal
            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.w3.org/2004/02/skos/core#altLabel> \"Southern Ireland\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-P < 100
            line = "<http://www.wikidata.org/entity/P27> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-P > 100 ; 
            line =
                "<http://www.wikidata.org/entity/P270> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-Q < 100; Predicate: Property; Object: Entity-Q < 100; 
            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //PASS: Subject: Entity-P ; Predicate: Label; Object: Literal;
            line = "<http://www.wikidata.org/entity/P22> <http://www.w3.org/2000/01/rdf-schema#label> \"father\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line, 100));
            Assert.True(TriplesFilter.IsValidLine(line, 0));

            //PASS: Subject: Entity-P ; Predicate: AltLabel; Object: Literal;
            line = "<http://www.wikidata.org/entity/P22> <http://www.w3.org/2004/02/skos/core#altLabel> \"dad\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));


            //PASS: Subject: Entity-P ; Predicate: Description; Object: Literal;
            line =
                "<http://www.wikidata.org/entity/P22> <http://schema.org/description> \"male parent of the subject. For stepfather, use \\\"stepparent\\\" (P3448)\"@en .";
            triple = line.ToTriple();
            Assert.True(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-Q > 100 ; 
            line =
                "<http://www.wikidata.org/entity/Q270> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@en .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-Q > 100; Predicate: Property; Object: Entity-Q < 100; 
            line =
                "<http://www.wikidata.org/entity/Q270> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-Q < 100; Predicate: Property; Object: Entity-Q > 100; 
            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q260> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.True(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-Q < 100; Predicate: Property; Object: Literal; 
            line = "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> \"Ireland\"@en .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-P ; Predicate: Other; Object: URI-Other;
            line =
                "<http://www.wikidata.org/entity/P22> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://wikiba.se/ontology-beta#Property> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-P ; Predicate: Other; Object: URI-Other;
            line =
                "<http://www.wikidata.org/entity/P5000> <http://wikiba.se/ontology-beta#propertyType> <http://wikiba.se/ontology-beta#WikibaseItem> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-P ; Predicate: Other; Object: Property;
            line =
                "<http://www.wikidata.org/entity/P22> <http://wikiba.se/ontology-beta#directClaim> <http://www.wikidata.org/prop/direct/P22> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-P ; Predicate: Other; Object: Literal;
            line =
                "<http://www.wikidata.org/entity/P22> <http://www.w3.org/2004/02/skos/core#prefLabel> \"father\"@en .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Subject: Entity-P ; Predicate: Property; Object: Entity;
            line =
                "<http://www.wikidata.org/entity/P22> <http://www.wikidata.org/prop/direct/P1659> <http://www.wikidata.org/entity/P25> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Predicate: Other; Object: Literal ValidLanguage;
            line =
                "<http://www.wikidata.org/entity/Q26> <http://www.w3.org/2004/02/skos/core#prefLabel> \"Northern Ireland\"@en .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));

            line = "<http://www.wikidata.org/entity/Q26> <http://schema.org/name> \"Northern Ireland\"@en .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));

            //FAIL: Predicate: Other; Object: Literal NotValidLanguage;
            line =
                "<http://www.wikidata.org/entity/Q26> <http://www.w3.org/2004/02/skos/core#prefLabel> \"Irlanda del Nord\"@it .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));

            //Italian Description
            line =
                "<http://www.wikidata.org/entity/P22> <http://schema.org/description> \"male parent of the subject. For stepfather, use \\\"stepparent\\\" (P3448)\"@it .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //Spanish Label
            line =
                "<http://www.wikidata.org/entity/P270> <http://www.w3.org/2000/01/rdf-schema#label> \"Ireland\"@es .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            //FAIL: Predicate: Other; Object: URI;
            line =
                "<http://www.wikidata.org/entity/Q26> <http://www.wikidata.org/prop/direct-normalized/P349> <http://id.ndl.go.jp/auth/ndlna/00566023> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            line =
                "<http://www.wikidata.org/entity/Q27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/T26> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            line =
                "<http://www.wikidata.org/entity/T27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/Q26> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            line =
                "<http://www.wikidata.org/entity/T27> <http://www.wikidata.org/prop/direct/P47> <http://www.wikidata.org/entity/T26> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));

            line =
                "<http://www.wikidata.org/entity/Q318> <http://www.wikidata.org/prop/direct/P1963> <http://www.wikidata.org/entity/P223> .";
            triple = line.ToTriple();
            Assert.False(TriplesFilter.IsValidLine(line,100));
            Assert.False(TriplesFilter.IsValidLine(line,0));
        }
    }
}