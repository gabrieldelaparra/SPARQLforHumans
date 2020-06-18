using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Logger;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Queries.Parsers;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index
{
    //public class PropertiesIndexerFromLucene
    //{
    //    private Dictionary<int, HashSet<int>> RangeDictionary { get; } = new Dictionary<int, HashSet<int>>();
    //    private Dictionary<int, HashSet<int>> DomainDictionary { get; } = new Dictionary<int, HashSet<int>>();

    //    public void Index()
    //    {

    //        ////RANGE:
    //        //foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
    //        //    var instanceOfIds = subjectGroup.Where(x => x.Predicate.IsInstanceOf()).Select(x => x.Object.GetIntId())
    //        //        .ToArray();
    //        //    var reverseProperties = subjectGroup
    //        //        .Where(x => x.Predicate.IsReverseProperty() && !x.Predicate.IsReverseInstanceOf()).ToArray();
    //        //    var reversePropertyIds = reverseProperties.Select(x => x.Predicate.GetIntId()).ToArray();
    //        //    var reverseInstanceOf = subjectGroup.Where(x => x.Predicate.IsReverseInstanceOf()).ToArray();

    //        //    if (reverseInstanceOf.Any())
    //        //        RangeDictionary.AddSafe(31, instanceOfIds);

    //        //    foreach (var reversePropertyId in reversePropertyIds) {
    //        //        RangeDictionary.AddSafe(reversePropertyId, instanceOfIds);
    //        //    }

    //        //    //LogMessage(readCount++, "Range", false);
    //        //}

    //        //LogMessage(readCount, "Range");
    //        //readCount = 0;


    //        ////DOMAIN:
    //        foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
    //            var directProperties = subjectGroup.Where(x => x.Predicate.IsProperty()).ToArray();
    //            var (instanceOf, otherProperties) = directProperties.SliceBy(x => x.Predicate.IsInstanceOf());
    //            var propertyIds = otherProperties.Select(x => x.Predicate.GetIntId()).ToArray();
    //            var instanceOfIds = instanceOf.Select(x => x.Object.GetIntId()).ToArray();
    //            DomainDictionary.AddSafe(31, instanceOfIds);
    //            foreach (var propertyId in propertyIds) {
    //                DomainDictionary.AddSafe(propertyId, instanceOfIds);
    //            }

    //            //LogMessage(readCount++, "Domain", false);
    //        }

    //        //LogMessage(readCount, "Domain");
    //        //readCount = 0;


    //        using var luceneDirectory = FSDirectory.Open(LuceneDirectoryDefaults.EntityIndexPath);
    //        using var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory);
    //        var docCount = luceneDirectoryReader.MaxDoc;
    //        for (var i = 0; i < docCount; i++)
    //        {
    //            var doc = luceneDirectoryReader.Document(i);
    //            var entity = doc.MapEntity();
    //            var reverseProperties = entity.ReverseProperties.Select(x => x.Id.ToInt());
    //            var properties = entity.Properties.Select(x => x.Id.ToInt());

    //            //TODO: Use constant:
    //            var otherProperties = properties.Where(x => !x.Equals(31));
    //            var types = entity.ParentTypes.Select(x => x.ToInt());
    //            var isType = entity.IsType;

    //            //TODO: Use constant:
    //            if (isType)
    //                RangeDictionary.AddSafe(31, types);

    //            foreach (var reversePropertyId in reverseProperties)
    //            {
    //                RangeDictionary.AddSafe(reversePropertyId, types);
    //            }

    //            DomainDictionary.AddSafe(31, types);

    //            foreach (var propertyId in otherProperties)
    //            {
    //                DomainDictionary.AddSafe(propertyId, types);
    //            }
    //        }
    //    }
    //}

    public class PropertiesIndexer : BaseNotifier, IIndexer
    {
        public PropertiesIndexer(string inputFilename, string outputDirectory, string entitiesIndexPath)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;
            EntitiesIndexPath = entitiesIndexPath;

            FieldIndexers = new List<IFieldIndexer<IIndexableField>> {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer()
            };
            NotifyTicks = 10000;
        }

        public string EntitiesIndexPath { get; set; }
        public override string NotifyMessage => "Build Properties Index";
        private Dictionary<int, HashSet<int>> DomainDictionary { get; } = new Dictionary<int, HashSet<int>>();
        private Hashtable FrequencyHashTable { get; } = new Hashtable();
        private Dictionary<int, HashSet<int>> RangeDictionary { get; } = new Dictionary<int, HashSet<int>>();
        public IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }
        public string InputFilename { get; set; }
        public string OutputDirectory { get; set; }

        public bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityP();
        }

        public void Index()
        {
            var indexConfig = LuceneIndexDefaults.CreateStandardIndexWriterConfig();

            long readCount = 0;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var subjectGroups = FileHelper.GetInputLines(InputFilename).GroupBySubject();

            using var luceneDirectory = FSDirectory.Open(EntitiesIndexPath);
            using var luceneDirectoryReader = DirectoryReader.Open(luceneDirectory);
            var docCount = luceneDirectoryReader.MaxDoc;
            for (var i = 0; i < docCount; i++)
            {
                var doc = luceneDirectoryReader.Document(i);
                var entity = doc.MapEntity();
                var reverseProperties = entity.ReverseProperties.Select(x => x.Id.ToInt());
                var properties = entity.Properties.Select(x => x.Id.ToInt());

                //TODO: Use constant:
                var otherProperties = properties.Where(x => !x.Equals(31));
                var types = entity.ParentTypes.Select(x => x.ToInt());
                var isType = entity.IsType;

                //TODO: Use constant:
                if (isType)
                    RangeDictionary.AddSafe(31, types);

                foreach (var reversePropertyId in reverseProperties)
                {
                    RangeDictionary.AddSafe(reversePropertyId, types);
                }

                DomainDictionary.AddSafe(31, types);

                foreach (var propertyId in otherProperties)
                {
                    DomainDictionary.AddSafe(propertyId, types);
                }
                LogMessage(readCount++, "Range", false);
            }

            ////RANGE:
            //foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
            //    var instanceOfIds = subjectGroup.Where(x => x.Predicate.IsInstanceOf()).Select(x => x.Object.GetIntId())
            //        .ToArray();
            //    var reverseProperties = subjectGroup
            //        .Where(x => x.Predicate.IsReverseProperty() && !x.Predicate.IsReverseInstanceOf()).ToArray();
            //    var reversePropertyIds = reverseProperties.Select(x => x.Predicate.GetIntId()).ToArray();
            //    var reverseInstanceOf = subjectGroup.Where(x => x.Predicate.IsReverseInstanceOf()).ToArray();

            //    if (reverseInstanceOf.Any())
            //        RangeDictionary.AddSafe(31, instanceOfIds);

            //    foreach (var reversePropertyId in reversePropertyIds) {
            //        RangeDictionary.AddSafe(reversePropertyId, instanceOfIds);
            //    }

            //    LogMessage(readCount++, "Range", false);
            //}

            //LogMessage(readCount, "Range");
            //readCount = 0;


            //////DOMAIN:
            //foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
            //    var directProperties = subjectGroup.Where(x => x.Predicate.IsProperty()).ToArray();
            //    var (instanceOf, otherProperties) = directProperties.SliceBy(x => x.Predicate.IsInstanceOf());
            //    var propertyIds = otherProperties.Select(x => x.Predicate.GetIntId()).ToArray();
            //    var instanceOfIds = instanceOf.Select(x => x.Object.GetIntId()).ToArray();
            //    DomainDictionary.AddSafe(31, instanceOfIds);
            //    foreach (var propertyId in propertyIds) {
            //        DomainDictionary.AddSafe(propertyId, instanceOfIds);
            //    }

            //    LogMessage(readCount++, "Domain", false);
            //}

            //LogMessage(readCount, "Domain");
            //readCount = 0;


            ////FREQUENCY
            foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
                var directProperties = subjectGroup.Where(x => x.Predicate.IsProperty()).ToArray();

                foreach (var triple in directProperties) {
                    var propertyIntId = triple.Predicate.GetIntId();

                    if (!FrequencyHashTable.ContainsKey(propertyIntId))
                        FrequencyHashTable.Add(propertyIntId, 0);

                    FrequencyHashTable[propertyIntId] = (int) FrequencyHashTable[propertyIntId] + 1;
                }

                LogMessage(readCount++, "Frequency", false);
            }

            LogMessage(readCount, "Frequency");
            readCount = 0;


            using (var indexDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            using (var writer = new IndexWriter(indexDirectory, indexConfig)) {
                foreach (var subjectGroup in subjectGroups.Where(FilterGroups)) {
                    var document = new Document();

                    foreach (var field in FrequencyGetField(subjectGroup)) {
                        document.Add(field);
                    }

                    foreach (var field in DomainGetField(subjectGroup)) {
                        document.Add(field);
                    }

                    foreach (var field in RangeGetField(subjectGroup)) {
                        document.Add(field);
                    }

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                        boost = (double) boostField.GetDoubleValue();

                    foreach (var fieldIndexer in FieldIndexers) {
                        fieldIndexer.Boost = boost;
                    }

                    foreach (var fieldIndexer in FieldIndexers) {
                        foreach (var field in fieldIndexer.GetField(subjectGroup)) {
                            document.Add(field);
                        }
                    }

                    LogProgress(readCount++);

                    writer.AddDocument(document);
                }
            }

            LogProgress(readCount, true);
        }

        public IEnumerable<StringField> DomainGetField(SubjectGroup subjectGroup)
        {
            var id = subjectGroup.Id.ToInt();
            return DomainDictionary.ContainsKey(id)
                ? DomainDictionary[id].Select(x =>
                    new StringField(Labels.DomainType.ToString(), x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }

        public IEnumerable<DoubleField> FrequencyGetField(SubjectGroup subjectGroup)
        {
            var subjectId = subjectGroup.Id.ToInt();
            return FrequencyHashTable.ContainsKey(subjectId)
                ? new List<DoubleField>
                    {new DoubleField(Labels.Rank.ToString(), (int) FrequencyHashTable[subjectId], Field.Store.YES)}
                : new List<DoubleField>();
        }

        public IEnumerable<StringField> RangeGetField(SubjectGroup subjectGroup)
        {
            var id = subjectGroup.Id.ToInt();
            return RangeDictionary.ContainsKey(id)
                ? RangeDictionary[id]
                    .Select(x => new StringField(Labels.Range.ToString(), x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }
    }
}