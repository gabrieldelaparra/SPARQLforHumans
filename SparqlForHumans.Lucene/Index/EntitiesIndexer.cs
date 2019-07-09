using Lucene.Net.Index;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Index
{
    public class EntitiesIndexer : BaseIndexer
    {
        public EntitiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            RelationMappers = new List<IFieldIndexer<IIndexableField>>
            {
                new EntityIsTypeIndexer(inputFilename),
                new EntityPageRankBoostIndexer(inputFilename)
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
        }

        public override string NotifyMessage => "Build Entities Index";

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityQ();
        }
    }
}