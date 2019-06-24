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
    public abstract class BaseIndexer : IIndexer
    {
        protected BaseIndexer(string inputFilename, string outputDirectory)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;
        }

        public string InputFilename { get; set; }
        public string OutputDirectory { get; set; }

        public abstract IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public abstract IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }

        public virtual void Index()
        {
            long readCount = 1;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var entityGroups = FileHelper.GetInputLines(InputFilename)
                .GroupBySubject()
                .Where(FilterGroups);

            var indexConfig = IndexConfiguration.CreateStandardIndexWriterConfig();

            using (var entitiesDirectory = FSDirectory.Open(OutputDirectory.GetOrCreateDirectory()))
            using (var writer = new IndexWriter(entitiesDirectory, indexConfig))
            {
                foreach (var entityGroup in entityGroups)
                {
                    var document = new Document();

                    foreach (var mapper in RelationMappers.Select(x => x.GetField(entityGroup)).Where(x => x != null))
                        document.Add(mapper);

                    var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
                    var boost = 0.0;
                    if (boostField != null)
                        boost = (double)boostField.GetDoubleValue();

                    foreach (var fieldIndexer in FieldIndexers)
                        fieldIndexer.Boost = boost;

                    foreach (var fields in FieldIndexers.Select(x => x.GetField(entityGroup)).Where(x => x != null))
                        document.Add(fields);

                    readCount++;

                    if (FilterGroups(entityGroup))
                        writer.AddDocument(document);
                }
            }
        }

        public abstract bool FilterGroups(SubjectGroup tripleGroup);
    }
}