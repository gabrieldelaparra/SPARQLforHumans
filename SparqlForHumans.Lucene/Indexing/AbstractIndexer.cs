using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Lucene.Indexing.EntityFields;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    public abstract class AbstractIndexer<TFieldTypes, TKey, TValue> : IIndexer<TFieldTypes, TKey, TValue>
        where TFieldTypes : IIndexableField

    {
        public abstract IEnumerable<IRelationMapper<IDictionary<TKey, TValue>>> RelationMappers { get; set; }
        public abstract IEnumerable<ISubjectGroupIndexer<TFieldTypes>> FieldIndexers { get; set; }
        public string InputFilename { get; set; }
        public string OutputDirectory { get; set; }

        protected AbstractIndexer(string inputFilename, string outputDirectory)
        {
            InputFilename = inputFilename;
            OutputDirectory = outputDirectory;
        }

        public virtual void Index()
        {
            long readCount = 1;
            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var lines = FileHelper.GetInputLines(InputFilename);
            var entityGroups = lines.GroupBySubject().Where(FilterGroups);

            // RUN RelationMappers:
            var list = new List<IsTypeIndexer>()
            {
                new IsTypeIndexer(entityGroups),
            };

            foreach (var entityGroup in entityGroups)
            {
                var document = new Document();
                foreach (var fields in FieldIndexers.Select(x => x.TriplesToField(entityGroup)).Where(x => x != null))
                    document.Add(fields);
                foreach (var field in list.Select(x=>x.GetField(entityGroup)).Where(x=>x != null))
                    document.Add(field);

                //if (entityPageRankDictionary.ContainsKey(rdfIndexEntity.Id.ToNumbers()))
                //    rdfIndexEntity.Rank = entityPageRankDictionary[rdfIndexEntity.Id.ToNumbers()];
                //if (typeEntitiesDictionary.ContainsKey(rdfIndexEntity.Id.ToNumbers()))
                //    rdfIndexEntity.IsType = true;
            }

            //foreach (var relationMapper in RelationMappers)
            //{
            //    //relationMapper.
            //}
        }

        public abstract bool FilterGroups(SubjectGroup tripleGroup);
    }
}
