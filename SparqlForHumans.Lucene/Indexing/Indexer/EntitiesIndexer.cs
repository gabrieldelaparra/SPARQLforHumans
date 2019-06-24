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
        public override string NotifyMessage => "Build Entities Index";

        public EntitiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            RelationMappers = new List<IFieldIndexer<IIndexableField>>
            {
                new IsTypeIndexer(inputFilename),
                new BoostIndexer(inputFilename)
            };

            //TODO: No 'prefLabel' in the current index:
            //TODO: No 'name' in the current index:
            FieldIndexers = new List<IFieldIndexer<IIndexableField>>
            {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer(),
                new InstanceOfIndexer(),
                new SubClassIndexer(),
                new EntityPropertiesIndexer()
            };

            //Index();
        }

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityQ();
        }
    }
}