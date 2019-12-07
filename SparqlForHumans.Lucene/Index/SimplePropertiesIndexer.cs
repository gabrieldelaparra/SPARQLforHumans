using System;
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
    public class SimplePropertiesIndexer: BaseNotifier, IIndexer
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
        private Dictionary<int, int> FrequencyDictionary { get; set; } = new Dictionary<int, int>();
        private Dictionary<int, List<int>> DomainDictionary { get; set; } = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> RangeDictionary { get; set; } = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> rangeAuxiliaryDictionary { get; set; } = new Dictionary<int, List<int>>();
        private string FrequencyFieldName => Labels.Rank.ToString();
        private string DomainFieldName => Labels.DomainType.ToString();
        private string RangeFieldName => Labels.Range.ToString();
        internal  void FrequencyParseTripleGroup(Dictionary<int, int> dictionary, IEnumerable<Triple> triples)
        {
            foreach (var triple in triples)
            {
                // Filter Properties Only
                if (!triple.Predicate.IsProperty()) continue;

                var predicateIntId = triple.Predicate.GetIntId();

                if (!dictionary.ContainsKey(predicateIntId))
                    dictionary.Add(predicateIntId, 0);

                dictionary[predicateIntId]++;
            }
        }
        
        internal void DomainParseTripleGroup(Dictionary<int, List<int>> dictionary, IEnumerable<Triple> triples)
        {
            // Filter those the triples that are properties only (Exclude description, label, etc.)
            var propertiesTriples = triples.Where(x => x.Predicate.IsProperty());

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).Distinct().ToArray();
            var instanceOfIds = instanceOfSlice.Select(x => x.Object.GetIntId()).Distinct().ToArray();
            var instanceOfPropertyIds = instanceOfSlice.Select(x => x.Predicate.GetIntId());

            foreach (var instanceOfId in instanceOfPropertyIds)
            {
                dictionary.AddSafe(instanceOfId, instanceOfIds);
            }

            foreach (var propertyId in propertyIds)
            {
                dictionary.AddSafe(propertyId, instanceOfIds);
            }
        }

        internal void RangeParseTripleGroup(Dictionary<int, List<int>> dictionary, IEnumerable<Triple> triples)
        {
            // Filter those the triples that are properties only (Exclude description, label, etc.)
            var propertiesTriples = triples.Where(x => x.Predicate.IsReverseProperty() 
                                                       || x.Predicate.IsInstanceOf() 
                                                       || x.Predicate.IsReverseInstanceOf()).ToArray();

            var instanceOf = propertiesTriples.Where(x => x.Predicate.IsInstanceOf());
            var reverseInstanceOf = propertiesTriples.Where(x => x.Predicate.IsReverseInstanceOf());
            var reverseProperties = propertiesTriples.Where(x => x.Predicate.IsReverseProperty() && !x.Predicate.IsReverseInstanceOf());

            var instanceOfIds = instanceOf.Select(x => x.Object.GetIntId());
            var reverseInstanceOfIds = reverseInstanceOf.Select(x => x.Predicate.GetIntId());
            var reversePropertyIds = reverseProperties.Select(x => x.Predicate.GetIntId());

            foreach (var reversePropertyId in reversePropertyIds) {
                dictionary.AddSafe(reversePropertyId, instanceOfIds);
            }

            foreach (var reverseInstanceOfId in reverseInstanceOfIds) {
                dictionary.AddSafe(reverseInstanceOfId, instanceOfIds);
            }
        }
        
        public IEnumerable<DoubleField> FrequencyGetField(SubjectGroup subjectGroup)
        {
            var subjectId = subjectGroup.Id.ToNumbers();
            return FrequencyDictionary.ContainsKey(subjectId)
                ? new List<DoubleField> { new DoubleField(FrequencyFieldName, FrequencyDictionary[subjectId], Field.Store.YES) }
                : new List<DoubleField>();
        }

        public IEnumerable<StringField> RangeGetField(SubjectGroup tripleGroup)
        {
            return RangeDictionary.ContainsKey(tripleGroup.Id.ToNumbers())
                ? RangeDictionary[tripleGroup.Id.ToNumbers()]
                    .Select(x => new StringField(RangeFieldName, x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }

        public IEnumerable<StringField> DomainGetField(SubjectGroup tripleGroup)
        {
            return DomainDictionary.ContainsKey(tripleGroup.Id.ToNumbers())
                ? DomainDictionary[tripleGroup.Id.ToNumbers()]
                    .Select(x => new StringField(DomainFieldName, x.ToString(), Field.Store.YES))
                : new List<StringField>();
        }

        public void Index()
        {
            long readCount = 0;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var subjectGroups = FileHelper.GetInputLines(InputFilename)
                .GroupBySubject();

            foreach (var subjectGroup in subjectGroups.Where(x => x.IsEntityQ())) {
                FrequencyParseTripleGroup(FrequencyDictionary, subjectGroup);
                DomainParseTripleGroup(DomainDictionary, subjectGroup);
                RangeParseTripleGroup(RangeDictionary, subjectGroup);
                LogProgress(readCount++);
            }

            readCount = 0;

            var indexConfig = LuceneIndexDefaults.CreateStandardIndexWriterConfig();

            using (var indexDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            using (var writer = new IndexWriter(indexDirectory, indexConfig))
            {
                foreach (var subjectGroup in subjectGroups.Where(FilterGroups).AsParallel())
                {
                    var document = new Document();

                    FrequencyGetField(subjectGroup).ToList().ForEach(x => document.Add(x));
                    DomainGetField(subjectGroup).ToList().ForEach(x => document.Add(x));
                    RangeGetField(subjectGroup).ToList().ForEach(x => document.Add(x));

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                    {
                        boost = (double)boostField.GetDoubleValue();
                    }

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

                    //if (FilterGroups(subjectGroup))
                    //{
                        writer.AddDocument(document);
                    //}
                }
            }

            LogProgress(readCount, true);
        }
    }
}
