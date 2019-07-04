using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Indexer
{
    public abstract class BaseIndexer : BaseNotifier, IIndexer
    {
        protected BaseIndexer(string inputFilename, string outputDirectory)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;
        }

        public string InputFilename { get; set; }
        public string OutputDirectory { get; set; }

        public IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }

        public virtual void Index()
        {
            long readCount = 0;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var subjectGroups = FileHelper.GetInputLines(InputFilename)
                .GroupBySubject()
                .Where(FilterGroups);

            var indexConfig = IndexConfiguration.CreateStandardIndexWriterConfig();

            using (var indexDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            using (var writer = new IndexWriter(indexDirectory, indexConfig))
            {
                foreach (var subjectGroup in subjectGroups)
                {
                    var document = new Document();

                    foreach (var mapper in RelationMappers.SelectMany(x => x.GetField(subjectGroup)))
                        document.Add(mapper);

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                        boost = (double)boostField.GetDoubleValue();

                    foreach (var fieldIndexer in FieldIndexers)
                        fieldIndexer.Boost = boost;

                    foreach (var fields in FieldIndexers.SelectMany(x => x.GetField(subjectGroup)))
                        document.Add(fields);

                    LogProgress(readCount++);

                    if (FilterGroups(subjectGroup))
                        writer.AddDocument(document);
                }
            }
            LogProgress(readCount, true);
        }

        public abstract bool FilterGroups(SubjectGroup tripleGroup);
    }
}