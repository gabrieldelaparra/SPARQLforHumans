using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Logger;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index
{
    public class SimplePropertiesIndexer : BaseNotifier, IIndexer
    {
        public SimplePropertiesIndexer(string inputFilename, string outputDirectory)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;

            FieldIndexers = new List<IFieldIndexer<IIndexableField>>
            {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer()
            };
        }
        public override string NotifyMessage => "Build Properties Index";
        public IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }
        public string InputFilename { get; set; }
        public string OutputDirectory { get; set; }
        public bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityP();
        }
        private Hashtable FrequencyHashTable { get; set; } = new Hashtable();
        private Dictionary<int, HashSet<int>> DomainDictionary { get; set; } = new Dictionary<int, HashSet<int>>();
        private Dictionary<int, HashSet<int>> RangeDictionary { get; set; } = new Dictionary<int, HashSet<int>>();
        private static string FrequencyFieldName => Labels.Rank.ToString();
        private static string DomainFieldName => Labels.DomainType.ToString();
        private static string RangeFieldName => Labels.Range.ToString();

        public IEnumerable<DoubleField> FrequencyGetField(SubjectGroup subjectGroup)
        {
            var subjectId = subjectGroup.Id.ToNumbers();
            return FrequencyHashTable.ContainsKey(subjectId)
                ? new List<DoubleField> { new DoubleField(FrequencyFieldName, (int)FrequencyHashTable[subjectId], Field.Store.YES) }
                : new List<DoubleField>();
        }

        public IEnumerable<StringField> RangeGetField(SubjectGroup subjectGroup)
        {
            return RangeDictionary.ContainsKey(subjectGroup.Id.ToNumbers())
                ? RangeDictionary[subjectGroup.Id.ToNumbers()]
                    .Select(x => new StringField(RangeFieldName, x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }

        public IEnumerable<StringField> DomainGetField(SubjectGroup subjectGroup)
        {
            return DomainDictionary.ContainsKey(subjectGroup.Id.ToNumbers())
                ? DomainDictionary[subjectGroup.Id.ToNumbers()]
                    .Select(x => new StringField(DomainFieldName, x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }

        public void Index()
        {
            var indexConfig = LuceneIndexDefaults.CreateStandardIndexWriterConfig();

            long readCount = 0;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var subjectGroups = FileHelper.GetInputLines(InputFilename)
                .GroupBySubject();

            //First Pass:
            foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
                
                var validTriples = subjectGroup.Where(x =>
                    x.Predicate.IsProperty() ||
                    (x.Predicate.IsReverseProperty() && !x.Predicate.IsReverseInstanceOf())).ToArray();

                var properties = validTriples.Where(x => x.Predicate.IsProperty()).ToArray();

                //FREQUENCY
                foreach (var triple in properties)
                {
                    var propertyIntId = triple.Predicate.GetIntId();

                    if (!FrequencyHashTable.ContainsKey(propertyIntId))
                        FrequencyHashTable.Add(propertyIntId, 0);

                    FrequencyHashTable[propertyIntId] = ((int)FrequencyHashTable[propertyIntId]) + 1;
                }

                //DOMAIN:
                var (instanceOf, otherProperties) = properties.SliceBy(x => x.Predicate.IsInstanceOf());
                var propertyIds = otherProperties.Select(x => x.Predicate.GetIntId());
                var instanceOfIds = instanceOf.Select(x => x.Object.GetIntId()).ToArray();
                DomainDictionary.AddSafe(31, instanceOfIds);
                foreach (var propertyId in propertyIds)
                    DomainDictionary.AddSafe(propertyId, instanceOfIds);

                //RANGE:
                var reverseProperties = validTriples.Where(x => x.Predicate.IsReverseProperty() && !x.Predicate.IsReverseInstanceOf());
                var reversePropertyIds = reverseProperties.Select(x => x.Predicate.GetIntId());
                RangeDictionary.AddSafe(31, instanceOfIds);
                foreach (var reversePropertyId in reversePropertyIds)
                    RangeDictionary.AddSafe(reversePropertyId, instanceOfIds);

                LogProgress(readCount++);
            }

            readCount = 0;

            //Second Pass:
            using (var indexDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            using (var writer = new IndexWriter(indexDirectory, indexConfig))
            {
                foreach (var subjectGroup in subjectGroups.Where(FilterGroups))
                {
                    var document = new Document();

                    foreach (var field in FrequencyGetField(subjectGroup))
                        document.Add(field);

                    foreach (var field in DomainGetField(subjectGroup))
                        document.Add(field);

                    foreach (var field in RangeGetField(subjectGroup))
                        document.Add(field);

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                        boost = (double)boostField.GetDoubleValue();

                    foreach (var fieldIndexer in FieldIndexers)
                        fieldIndexer.Boost = boost;

                    foreach (var fieldIndexer in FieldIndexers)
                        foreach (var field in fieldIndexer.GetField(subjectGroup))
                            document.Add(field);

                    LogProgress(readCount++);

                    writer.AddDocument(document);
                }
            }

            LogProgress(readCount, true);
        }
    }
}
