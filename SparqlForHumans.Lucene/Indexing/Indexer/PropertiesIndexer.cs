using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Fields;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Indexing.Indexer
{
    public class PropertiesIndexer : BaseIndexer
    {
        public PropertiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            FieldIndexers = new List<IFieldIndexer<IIndexableField>>
            {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer()
            };

            RelationMappers = new List<IFieldIndexer<IIndexableField>>
            {
                new FrequencyIndexer(inputFilename),
                new PropertyDomainIndexer(inputFilename)
            };
        }

        public override IEnumerable<IFieldIndexer<IIndexableField>> RelationMappers { get; set; }
        public override IEnumerable<IFieldIndexer<IIndexableField>> FieldIndexers { get; set; }

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return true;
        }
    }
}