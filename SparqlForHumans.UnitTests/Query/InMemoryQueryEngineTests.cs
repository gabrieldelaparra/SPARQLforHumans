﻿using System;
using System.Linq;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries.Graph;
using SparqlForHumans.Utilities;
using Xunit;

namespace SparqlForHumans.UnitTests.Query
{
    public class InMemoryQueryEngineTests : IDisposable
    {
        private const string Filename = @"Resources/QueryGraphInMemoryEngine.nt";
        private const string EntitiesIndexPath = "QueryGraphInMemoryEngineEntities";
        private const string PropertiesIndexPath = "QueryGraphInMemoryEngineProperties";
        private InMemoryQueryEngine InMemoryQueryEngine;

        public InMemoryQueryEngineTests()
        {
            InMemoryQueryEngine = new InMemoryQueryEngine();
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();

            new EntitiesIndexer(Filename, EntitiesIndexPath).Index();
            new PropertiesIndexer(Filename, PropertiesIndexPath, EntitiesIndexPath).Index();

            InMemoryQueryEngine.Init(EntitiesIndexPath, PropertiesIndexPath);
        }

        public void Dispose()
        {
            EntitiesIndexPath.DeleteIfExists();
            PropertiesIndexPath.DeleteIfExists();
        }

        [Fact]
        public void TestOutgoingProperties()
        {
            var actual = InMemoryQueryEngine.BatchEntityIdOutgoingPropertiesQuery(new[] { "http://www.wikidata.org/entity/Q5" }).ToArray();

            Assert.Contains("P25", actual);
            Assert.Contains("P27", actual);
            Assert.Contains("P777", actual);
        }

        [Fact]
        public void TestIncomingProperties()
        {
            var actual = InMemoryQueryEngine.BatchEntityIdIncomingPropertiesQuery(new[] { "http://www.wikidata.org/entity/Q5" });

            Assert.Contains("P25", actual);
        }

        [Fact]
        public void TestDomainEntitiesInstanceOf()
        {
            var actual = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(new[] { "http://www.wikidata.org/prop/direct/P31" }).ToArray();

            Assert.Equal(8, actual.Length);
            Assert.Contains("Q5", actual);
            Assert.Contains("Q49088", actual);
        }

        [Fact]
        public void TestRangeEntitiesInstanceOf()
        {
            var actual = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(new[] { "http://www.wikidata.org/prop/direct/P31" }).ToArray();

            Assert.Equal(8, actual.Length);
            Assert.Contains("Q100", actual);
            Assert.Contains("Q300", actual);
        }

        [Fact]
        public void TestDomainEntities()
        {
            var actual = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(new[] { "http://www.wikidata.org/prop/direct/P25" }).ToArray();

            Assert.Equal(2, actual.Length);
            Assert.Contains("Q5", actual);
            Assert.Contains("Q49088", actual);
        }

        [Fact]
        public void TestRangeEntities()
        {
            var actual = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(new[] { "http://www.wikidata.org/prop/direct/P27" }).ToArray();

            Assert.Single(actual);
            Assert.Contains("Q6256", actual);
        }


    }
}
