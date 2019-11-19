using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Directory = System.IO.Directory;

namespace SparqlForHumans.UnitTests.Query
{
    public class QueryTests
    {
        [Fact]
        public void TestFullQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelEntityQuery(outputPath, "Obama").Query().FirstOrDefault();
            Debug.Assert(actual != null, nameof(actual) + " != null");
            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestMultiQueryBarackObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexBarack";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelEntityQuery(outputPath, "Obama").Query().FirstOrDefault();
            Debug.Assert(actual != null, nameof(actual) + " != null");
            Assert.Equal("Q76", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestMultiQueryMichelleObamaShouldShowFirst()
        {
            const string filename = "Resources/QueryMulti.nt";
            const string outputPath = "QueryMultiIndexMichelle";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new MultiLabelEntityQuery(outputPath, "Michelle Obama").Query().FirstOrDefault();

            Debug.Assert(entity != null, nameof(entity) + " != null");
            Assert.Equal("Q13133", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestNoEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardHalfWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelEntityQuery(outputPath, "Oba").Query().FirstOrDefault();

            Debug.Assert(actual != null, nameof(actual) + " != null");
            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryAddProperties()
        {
            const string outputPath = "Resources/PropertyIndex";
            Assert.True(Directory.Exists(outputPath));

            var entity = new SingleIdEntityQuery(outputPath, "Q26").Query().FirstOrDefault();

            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            //Properties
            Assert.Equal(4, entity.Properties.Count());

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal(string.Empty, entity.Properties.ElementAt(3).Label);

            entity.AddProperties(outputPath);

            Assert.Equal("P17", entity.Properties.ElementAt(0).Id);
            Assert.Equal("country", entity.Properties.ElementAt(0).Label);

            Assert.Equal("P47", entity.Properties.ElementAt(1).Id);
            Assert.Equal("shares border with", entity.Properties.ElementAt(1).Label);

            Assert.Equal("P30", entity.Properties.ElementAt(2).Id);
            Assert.Equal("continent", entity.Properties.ElementAt(2).Label);

            Assert.Equal("P131", entity.Properties.ElementAt(3).Id);
            Assert.Equal("located in the administrative territorial entity", entity.Properties.ElementAt(3).Label);
        }

        [Fact]
        public void TestQueryByMultipleIds()
        {
            var ids = new List<string> { "Q26", "Q27", "Q29" };
            const string indexPath = "Resources/IndexMultiple";
            Assert.True(Directory.Exists(indexPath));

            var entities = new BatchIdEntityQuery(indexPath, ids).Query();

            Assert.Equal(3, entities.Count());

            //Q26, Q27, Q29
            var doc = entities.ElementAt(0);
            Assert.NotNull(doc);
            Assert.Equal("Q26", doc.Id);
            Assert.Equal("Northern Ireland", doc.Label);

            doc = entities.ElementAt(1);
            Assert.NotNull(doc);
            Assert.Equal("Q27", doc.Id);
            Assert.Equal("Ireland", doc.Label);

            doc = entities.ElementAt(2);
            Assert.NotNull(doc);
            Assert.Equal("Q29", doc.Id);
            Assert.Equal("Spain", doc.Label);
        }

        [Fact]
        public static void TestQueryIsTypeFields()
        {
            const string filename = "Resources/TypeProperties.nt";
            const string outputPath = "CreateIndexIsTypeFields";

            outputPath.DeleteIfExists();
            Assert.False(Directory.Exists(outputPath));

            new EntitiesIndexer(filename, outputPath).Index();

            var query = "chile";
            var types = new MultiLabelTypeQuery(outputPath, query).Query();
            var all = new MultiLabelEntityQuery(outputPath, query).Query();

            Assert.Empty(types);
            Assert.Single(all);

            query = "country";
            types = new MultiLabelTypeQuery(outputPath, query).Query();
            all = new MultiLabelEntityQuery(outputPath, query).Query();

            Assert.Single(types);
            Assert.Single(all);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestQueryNonExistingEntityById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdEntityQuery(indexPath, "Q666").Query();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingEntityByLabel()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleLabelEntityQuery(indexPath, "Non-Existing").Query();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingPropertyById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdPropertyQuery(indexPath, "P666").Query();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQueryNonExistingPropertyByLabel()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleLabelPropertyQuery(indexPath, "Non-Existing").Query();

            Assert.Empty(actual);
        }

        [Fact]
        public void TestQuerySingleInstanceById()
        {
            const string indexPath = "Resources/IndexSingle";
            var actual = new SingleIdEntityQuery(indexPath, "Q26").Query().FirstOrDefault();

            Assert.NotNull(actual);
            Assert.Equal("Q26", actual.Id);
        }

        [Fact]
        public void TestQuerySingleInstanceByLabel()
        {
            const string indexPath = "Resources/IndexSingle";

            var entity = new SingleLabelEntityQuery(indexPath, "Northern Ireland").Query().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelEntityQuery(indexPath, "Ireland").Query().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelEntityQuery(indexPath, "Northern").Query().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);

            entity = new SingleLabelEntityQuery(indexPath, "north").Query().FirstOrDefault();
            Assert.NotNull(entity);
            Assert.Equal("Q26", entity.Id);
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithAltLabels_Q1More()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabelsQ1More.nt";
            const string outputPath = "QueryRanksAllWithAltLabelsQ1SortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();

            var entities = new MultiLabelEntityQuery(outputPath, "EntityQ").Query().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_AllWithSameAltLabels()
        {
            const string filename = "Resources/QueryRanksAllWithAltLabels.nt";
            const string outputPath = "QueryRanksAllSameAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelEntityQuery(outputPath, "EntityQ").Query().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OneWithAltLabels()
        {
            const string filename = "Resources/QueryRanksOneWithAltLabels.nt";
            const string outputPath = "QueryRanksOneAltLabelsSortedByPageRank";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelEntityQuery(outputPath, "EntityQ").Query().ToArray();

            // Had to fix these tests to take PageRank and Boost altogether to pass.
            Assert.Equal("Q1", entities[0].Id); //0.138
            Assert.Equal("Q6", entities[1].Id); //0.222
            Assert.Equal("Q4", entities[2].Id); //0.180
            Assert.Equal("Q7", entities[3].Id); //0.180
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyAltLabels()
        {
            const string filename = "Resources/QueryRanksOnlyAltLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyAltLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelEntityQuery(outputPath, "EntityQ").Query().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestRankMultiQuery_ShouldBeSortedByRank_OnlyLabels()
        {
            const string filename = "Resources/QueryRanksOnlyLabels.nt";
            const string outputPath = "QueryRanksSortedByPageRankOnlyLabels";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entities = new MultiLabelEntityQuery(outputPath, "EntityQ").Query().ToArray();

            Assert.Equal("Q6", entities[0].Id); //0.222
            Assert.Equal("Q4", entities[1].Id); //0.180
            Assert.Equal("Q7", entities[2].Id); //0.180
            Assert.Equal("Q1", entities[3].Id); //0.138
            Assert.Equal("Q5", entities[4].Id); //0.128
            Assert.Equal("Q2", entities[5].Id); //0.087
            Assert.Equal("Q3", entities[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleQuery_BarackObama_ShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexBarack";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new SingleLabelEntityQuery(outputPath, "Obama").Query().FirstOrDefault();

            Debug.Assert(entity != null, nameof(entity) + " != null");
            Assert.Equal("Q76", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestSingleQuery_MichelleObama_ShouldShowFirst()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QuerySingleIndexMichelle";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var entity = new SingleLabelEntityQuery(outputPath, "Michelle Obama").Query().FirstOrDefault();

            Debug.Assert(entity != null, nameof(entity) + " != null");
            Assert.Equal("Q13133", entity.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestInstanceOfQuery_InstancesOfHumans()
        {
            const string filename = "Resources/QuerySingle.nt";
            const string outputPath = "QueryInstanceOf";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var q5Entities = new MultiIdInstanceOfEntityQuery(outputPath, "Q5").Query();

            Assert.True(q5Entities.All(x=>x.InstanceOf.Contains("Q5")));

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestTopQueryEntitiesResults()
        {
            const string filename = "Resources/QueryEntityWildcardAllResults.nt";
            const string outputPath = "AllEntitiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelEntityQuery(outputPath, "*").Query().ToArray();

            Assert.NotEmpty(actual);
            Assert.Equal("Q6", actual[0].Id); //0.222
            Assert.Equal("Q4", actual[1].Id); //0.180
            Assert.Equal("Q7", actual[2].Id); //0.180
            Assert.Equal("Q1", actual[3].Id); //0.138
            Assert.Equal("Q5", actual[4].Id); //0.128
            Assert.Equal("Q2", actual[5].Id); //0.087
            Assert.Equal("Q3", actual[6].Id); //0.061

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestTopQueryPropertiesResults()
        {
            const string filename = @"Resources/QueryPropertyWildcardAllResults.nt";
            const string outputPath = "AllPropertiesResultsWildcardQueriesFullWord";

            outputPath.DeleteIfExists();

            new PropertiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelPropertyQuery(outputPath, "*").Query().ToArray();

            Assert.NotEmpty(actual);
            Assert.Equal("P530", actual[0].Id); //50
            Assert.Equal("P47", actual[1].Id); //5
            Assert.Equal("P17", actual[2].Id); //3
            Assert.Equal("P30", actual[3].Id); //3

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestWithEndWildcardQueryResults()
        {
            const string filename = "Resources/QueryWildcardOnePerLetter.nt";
            const string outputPath = "OneLetterWildcardWithAsterisk";

            outputPath.DeleteIfExists();

            new EntitiesIndexer(filename, outputPath).Index();
            var actual = new MultiLabelEntityQuery(outputPath, "Oba*").Query().FirstOrDefault();

            Debug.Assert(actual != null, nameof(actual) + " != null");
            Assert.Equal("Q76000000", actual.Id);

            outputPath.DeleteIfExists();
        }

        [Fact]
        public void TestScenario2GetDomainsForUnknownObjectType()
        {
            /* In this test, I will have two "nodes" connected.
             * "node1" is InstanceOf Human and has a second property to "node2"
             * "node2" is unkown type.
             * I want to display all the properties that have Domain Human.
             * 
             * As sample I have the following:
             * Q76(Obama) -> P31(Type) -> Q5(Human)
             * Q76(Obama) -> P27 -> Qxx
             * Q76(Obama) -> P555 -> Qxx
             */

            const string filename = @"Resources/QueryByDomain.nt";
            const string propertyOutputPath = "QueryByDomain";
            propertyOutputPath.DeleteIfExists();

            //Act:
            new PropertiesIndexer(filename, propertyOutputPath).Index();
            var domainProperties = new MultiDomainPropertyQuery(propertyOutputPath, "Q5").Query();

            Assert.NotEmpty(domainProperties);
            Assert.Equal(2, domainProperties.Count()); //P27, P555
            Assert.Equal("P27", domainProperties[0].Id);
            Assert.Equal("P555", domainProperties[1].Id);

            propertyOutputPath.DeleteIfExists();
        }

        [Fact]
        public void TestScenario3GetRangesForUnknownSubjectType()
        {
            /* In this test, I will have two "nodes" connected.
             * "node1" is unkown type and has a property to "node2"
             * "node2" is InstanceOf (Human).
             * I want to display all the properties that have Range Human.
             * 
             * As sample database I have the following:
             * Qxx (Mother) -> P25 (MotherOf) -> Qyy
             * Qxx (Mother) -> P25 (MotherOf) -> Qzz
             * ...
             * Qyy -> P31 (Type) -> Q5 (Human)
             * ```
             * El Range que debe mostrar que:
             * ```
             * Q5: Range P25
             */

            const string filename = @"Resources/QueryByRange.nt";
            const string propertyOutputPath = "QueryByRange";
            propertyOutputPath.DeleteIfExists();

            //Act:
            new PropertiesIndexer(filename, propertyOutputPath).Index();
            var rangeEntities = new MultiRangePropertyQuery(propertyOutputPath, "Q5").Query();

            Assert.NotEmpty(rangeEntities);
            Assert.Single(rangeEntities); // P25
            Assert.Equal("P25", rangeEntities[0].Id);

            propertyOutputPath.DeleteIfExists();
        }

        [Fact]
        public void TestScenario4GetPropertiesForKnownSubjectObjectType()
        {
            /* In this test, I will have two "nodes" connected.
             * "node1" is unkown type and has a property to "node2"
             * "node2" is InstanceOf (Human).
             * I want to display all the properties that have Range Human.
             * 
             * As sample database I have the following:
             * Q76 (Obama) -> P31 (InstanceOf) -> Q5 (Human)
             * Q76 (Obama) -> P27 (CountryOfCitizenship) -> Q30 (USA)
             * Q76 (Obama) -> Pxx (Others) -> Qxx
             * 
             * Q30 (USA) -> P31 (InstanceOf) -> Q6256 (Country)
             * Q30 (USA) -> Pyy (Others) -> Qyy
             * ...
             * Las propiedades que se deben mostrar que:
             * ```
             * Q30: Domain P27
             * Q5: Range P27
             */

            const string filename = @"Resources/QueryByRangeAndProperty.nt";
            const string propertyOutputPath = "QueryByRangeAndProperty";
            propertyOutputPath.DeleteIfExists();

            //Act:
            new PropertiesIndexer(filename, propertyOutputPath).Index();
            var rangeProperties = new MultiRangePropertyQuery(propertyOutputPath, "Q6256").Query();
            var domainProperties = new MultiDomainPropertyQuery(propertyOutputPath, "Q5").Query();
            var properties = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToArray();

            Assert.NotEmpty(properties);
            Assert.Single(properties); // P27
            Assert.Equal("P27", properties[0].Id);

            propertyOutputPath.DeleteIfExists();
        }

        [Fact]
        public void TestScenario4GetPropertiesForKnownSubjectObjectTypeWithGarbage()
        {
            /* In this test, I will have two "nodes" connected.
             * "node1" is unkown type and has a property to "node2"
             * "node2" is InstanceOf (Human).
             * I want to display all the properties that have Range Human.
             * 
             * As sample database I have the following:
             * Q76 (Obama) -> P31 (InstanceOf) -> Q5 (Human)
             * Q76 (Obama) -> P27 (CountryOfCitizenship) -> Q30 (USA)
             * Q76 (Obama) -> Pxx (Others) -> Qxx
             * 
             * Q30 (USA) -> P31 (InstanceOf) -> Q6256 (Country)
             * Q30 (USA) -> Pyy (Others) -> Qyy
             * ...
             * Las propiedades que se deben mostrar que:
             * ```
             * Q30: Domain P27
             * Q5: Range P27
             */

            const string filename = @"Resources/QueryByRangeAndProperty-More.nt";
            const string propertyOutputPath = "QueryByRangeAndPropertyMore";
            propertyOutputPath.DeleteIfExists();

            //Act:
            new PropertiesIndexer(filename, propertyOutputPath).Index();
            var rangeProperties = new MultiRangePropertyQuery(propertyOutputPath, "Q6256").Query();
            Assert.Equal(2, rangeProperties.Count); //P27, P555
            var domainProperties = new MultiDomainPropertyQuery(propertyOutputPath, "Q5").Query();
            Assert.Equal(3, domainProperties.Count); // P31, P27, P777
            var properties = rangeProperties.Intersect(domainProperties,new PropertyComparer()).ToArray();

            Assert.NotEmpty(properties);
            Assert.Single(properties); // P27
            Assert.Equal("P27", properties[0].Id);

            propertyOutputPath.DeleteIfExists();
        }
    }
}