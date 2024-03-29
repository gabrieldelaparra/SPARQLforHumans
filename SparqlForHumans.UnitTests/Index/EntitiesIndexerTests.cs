﻿using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Index
{
    public class EntitiesIndexerTests
    {
        [Fact]
        public void TestCreateEntityIndexAddsFolders()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateEntityIndexAddsFolders";
            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));
            new EntitiesIndexer(filename, outputPath).Index();
            Assert.True(Directory.Exists(outputPath));
        }

        [Fact]
        public void TestCreateSingleInstanceIndexAltLabel()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexAltLabel";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc = reader.Document(0);
            Assert.Equal(3, doc.GetValues(Labels.AltLabel).Length);

            Assert.Equal("NIR", doc.GetValues(Labels.AltLabel)[0]);
            Assert.Equal("UKN", doc.GetValues(Labels.AltLabel)[1]);
            Assert.Equal("North Ireland", doc.GetValues(Labels.AltLabel)[2]);
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDescription()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexDescription";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc = reader.Document(0);
            Assert.Equal("region in north-west Europe, part of the United Kingdom",
                doc.GetValue(Labels.Description));
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocCount()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexDocCount";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var docCount = reader.MaxDoc;
            Assert.Equal(1, docCount);
        }

        [Fact]
        public void TestCreateSingleInstanceIndexId()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexId";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc = reader.Document(0);
            Assert.Equal("Q26", doc.GetValue(Labels.Id));
        }

        [Fact]
        public void TestCreateSingleInstanceIndexLabel()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexLabel";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc = reader.Document(0);
            Assert.Equal("Northern Ireland", doc.GetValue(Labels.Label));
        }

        [Fact]
        public void TestCreateSingleInstanceIndexProperties()
        {
            const string filename = "Resources/EntityIndexSingleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexProperties";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc = reader.Document(0);

            Assert.Equal(5, doc.GetValues(Labels.Property).Length);

            Assert.Equal("P31", doc.GetValues(Labels.Property)[0]);
            Assert.Equal("P17", doc.GetValues(Labels.Property)[1]);
            Assert.Equal("P47", doc.GetValues(Labels.Property)[2]);
            Assert.Equal("P279", doc.GetValues(Labels.Property)[3]);
            Assert.Equal("P131", doc.GetValues(Labels.Property)[4]);
        }
 
        [Fact]
        public void TestCreateEntityMultipleInstanceIndex()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "CreateEntityMultipleInstanceIndex";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var docCount = reader.MaxDoc;

            Assert.Equal(3, docCount);

            //Q26, Q27, Q29
            var doc = reader.Document(0);
            Assert.NotNull(doc);
            Assert.Equal("Q26", doc.GetField(Labels.Id.ToString()).GetStringValue());
            Assert.Equal("Northern Ireland", doc.GetField(Labels.Label.ToString()).GetStringValue());

            doc = reader.Document(1);
            Assert.NotNull(doc);
            Assert.Equal("Q27", doc.GetField(Labels.Id.ToString()).GetStringValue());
            Assert.Equal("Ireland", doc.GetField(Labels.Label.ToString()).GetStringValue());

            doc = reader.Document(2);
            Assert.NotNull(doc);
            Assert.Equal("Q29", doc.GetField(Labels.Id.ToString()).GetStringValue());
            Assert.Equal("Spain", doc.GetField(Labels.Label.ToString()).GetStringValue());
        }

        [Fact]
        public void TestCreateEntityMultipleInstanceIndexDocCount()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "CreateEntityMultipleInstanceIndexDocCount";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();

            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var docCount = reader.MaxDoc;
            Assert.Equal(3, docCount);
        }

        [Fact]
        public void TestCreateSingleInstanceIndexDocNotNull()
        {
            const string filename = "Resources/EntityIndexMultipleInstance.nt";
            const string outputPath = "CreateSingleInstanceIndexDocNotNull";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory());
            using var reader = DirectoryReader.Open(luceneDirectory);
            var doc1 = reader.Document(0);
            var doc2 = reader.Document(1);
            var doc3 = reader.Document(2);

            Assert.NotNull(doc1);
            Assert.NotNull(doc2);
            Assert.NotNull(doc3);
        }

        [Fact]
        public void TestCreateEntityIndex5k()
        {
            const string filename = "Resources/EntityIndex5k.nt";
            const string outputPath = "CreateEntityIndex5k";
            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            Assert.True(Directory.Exists(outputPath));

            var queryBerlin = new SingleLabelEntityQuery(outputPath, "Berli").Query().ToArray();
            Assert.NotEmpty(queryBerlin);
            var result = queryBerlin[0];
            Assert.Equal("Q64", result.Id);
            Assert.Equal("Berlin", result.Label);
            Assert.Equal("capital city of Germany", result.Description);
            Assert.Contains("Berlin, Germany", result.AltLabels);
            var properties = result.Properties.Select(x => x.Id).ToArray();
            Assert.Contains("P17", properties);
            Assert.Contains("P1376", properties);
            Assert.False(result.IsType);
            Assert.Contains("Q515", result.ParentTypes);
            Assert.Contains("Q5119", result.ParentTypes);
            //Assert.Contains("Q999", result.SubClass);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public static void TestCreateIndexAddTypesFields()
        {
            const string filename = "Resources/EntityIndexTypes.nt";
            const string outputPath = "CreateIndexAddTypesFields";
            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();
            var obama = new SingleIdEntityQuery(outputPath, "Q76").Query().FirstOrDefault();
            var person = new SingleIdEntityQuery(outputPath, "Q5").Query().FirstOrDefault();
            var country = new SingleIdEntityQuery(outputPath, "Q17").Query().FirstOrDefault();
            var chile = new SingleIdEntityQuery(outputPath, "Q298").Query().FirstOrDefault();

            Assert.Equal("Q76", obama.Id);
            Assert.Equal("Q5", person.Id);
            Assert.Equal("Q17", country.Id);
            Assert.Equal("Q298", chile.Id);

            Assert.False(obama.IsType);
            Assert.False(chile.IsType);
            Assert.True(person.IsType);
            Assert.True(country.IsType);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateIndexDocumentCountEqualsGroupsCounts()
        {
            var filename = "Resources/Filter500.nt";
            var lines = FileHelper.GetInputLines(filename);
            var groups = lines.GroupBySubject();
            var entitiesCount = groups.Count();

            var outputPath = "CreateIndexDocumentCountEqualsGroupsCounts";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneIndexDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneIndexDirectory))
                {
                    Assert.Equal(entitiesCount, reader.NumDocs);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexInstanceOf()
        {
            const string filename = "Resources/EntityIndexTwoInstanceOf.nt";
            const string outputPath = "CreateSingleInstanceIndexInstanceOf";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    var instanceOfArray = doc.GetValues(Labels.InstanceOf);
                    Assert.NotNull(instanceOfArray);
                    Assert.Equal(2, instanceOfArray.Length);
                    Assert.Equal("Q145", instanceOfArray[0]);
                    Assert.Equal("Q27", instanceOfArray[1]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexIsType()
        {
            const string filename = "Resources/EntityIndexTwoInstanceOfWithTypes.nt";
            const string outputPath = "CreateSingleInstanceIndexIsType";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);
                    Assert.Empty(doc.GetValue(Labels.IsTypeEntity));
                    doc = reader.Document(1);
                    Assert.True(bool.Parse(doc.GetValue(Labels.IsTypeEntity)));
                    doc = reader.Document(2);
                    Assert.True(bool.Parse(doc.GetValue(Labels.IsTypeEntity)));
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestCreateSingleInstanceIndexReverseProperties()
        {
            const string filename = "Resources/EntityIndexMultipleInstanceReverse.nt";
            const string outputPath = "CreateSingleInstanceIndexReverseProperties";
            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            using (var luceneDirectory = FSDirectory.Open(outputPath.GetOrCreateDirectory()))
            {
                using (var reader = DirectoryReader.Open(luceneDirectory))
                {
                    var doc = reader.Document(0);

                    Assert.Equal(2, doc.GetValues(Labels.ReverseProperty).Length);

                    Assert.Equal("P47", doc.GetValues(Labels.ReverseProperty)[0]);
                    Assert.Equal("P48", doc.GetValues(Labels.ReverseProperty)[1]);
                }
            }

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestIndexHasTypes()
        {
            const string filename = "Resources/EntityTypes.nt";
            const string outputPath = "IndexHasTypes";
            outputPath.DeleteIfExists();

            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            var typesQuery = new MultiIdInstanceOfEntityQuery(outputPath, "Q5").Query();

            Assert.NotEmpty(typesQuery);
            Assert.All(typesQuery, x => x.ParentTypes.Equals("Q5"));

            outputPath.DeleteIfExists();
        }
    }
}