using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Fields;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Indexer
{
    public class EntitiesIndexer : BaseIndexer
    {
        public EntitiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            RelationMappers = new List<IFieldIndexer<IIndexableField>>
            {
                new IsTypeIndexer(inputFilename),
                new BoostIndexer(inputFilename)
            };

            FieldIndexers = new List<IFieldIndexer<IIndexableField>>
            {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer(),
                new InstanceOfIndexer(),
                new SubClassIndexer(),
                new PropertiesIndexer()
            };
        }

        //public sealed override IEnumerable<IRelationMapper<TDictionary>> RelationMappers { get; set; }
        public sealed override IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public sealed override IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityQ();
        }
    }
}