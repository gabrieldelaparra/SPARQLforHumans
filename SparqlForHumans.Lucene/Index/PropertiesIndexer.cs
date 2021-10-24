using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

using SparqlForHumans.Logger;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index
{
    public class PropertiesIndexer : BaseNotifier, IIndexer
    {
        public PropertiesIndexer(string inputFilename, string outputDirectory, string entitiesIndexPath)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;
            EntitiesIndexPath = entitiesIndexPath;

            if (!System.IO.Directory.Exists(EntitiesIndexPath))
            {
                LogInfo("Please build entities index first");
                return;
            }

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
                var reverseProperties = entity.ReverseProperties.Select(x => x.Id.ToInt()).ToList();
                var properties = entity.Properties.Select(x => x.Id.ToInt()).ToList();

                //TODO: Use constant:
                var otherProperties = properties.Where(x => !x.Equals(31)).ToList();
                var types = entity.ParentTypes.Select(x => x.ToInt()).ToList();
                var isType = entity.IsType;

                //Range
                //TODO: Use constant:
                //if (isType)
                RangeDictionary.AddSafe(31, types);

                foreach (var reversePropertyId in reverseProperties)
                {
                    RangeDictionary.AddSafe(reversePropertyId, types);
                }

                //Domain
                DomainDictionary.AddSafe(31, types);

                foreach (var propertyId in otherProperties)
                {
                    DomainDictionary.AddSafe(propertyId, types);
                }

                //Frequency
                foreach (var propertyIntId in properties)
                {
                    if (!FrequencyHashTable.ContainsKey(propertyIntId))
                        FrequencyHashTable.Add(propertyIntId, 0);
                    FrequencyHashTable[propertyIntId] = (int)FrequencyHashTable[propertyIntId] + 1;
                }

                LogMessage(readCount++, "Frequency, Domain, Range", false);
            }
            LogMessage(readCount, "Frequency, Domain, Range");
            readCount = 0;

            using (var indexDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            {
                using var writer = new IndexWriter(indexDirectory, indexConfig);
                foreach (var subjectGroup in subjectGroups.Where(FilterGroups))
                {
                    var document = new Document();

                    foreach (var field in FrequencyGetField(subjectGroup))
                    {
                        document.Add(field);
                    }

                    foreach (var field in DomainGetField(subjectGroup))
                    {
                        document.Add(field);
                    }

                    foreach (var field in RangeGetField(subjectGroup))
                    {
                        document.Add(field);
                    }

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                        boost = (double)boostField.GetDoubleValue();

                    foreach (var fieldIndexer in FieldIndexers)
                    {
                        fieldIndexer.Boost = boost;
                    }

                    foreach (var fieldIndexer in FieldIndexers)
                    {
                        foreach (var field in fieldIndexer.GetField(subjectGroup))
                        {
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
            var subjectId = subjectGroup.Id.ToInt();
            return DomainDictionary.ContainsKey(subjectId)
                ? DomainDictionary[subjectId].Select(x =>
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
            var subjectId = subjectGroup.Id.ToInt();
            return RangeDictionary.ContainsKey(subjectId)
                ? RangeDictionary[subjectId]
                    .Select(x => new StringField(Labels.Range.ToString(), x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }
    }
}