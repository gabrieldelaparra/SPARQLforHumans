using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.Base;
using SparqlForHumans.Lucene.Indexing.Fields;
using SparqlForHumans.Lucene.Indexing.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Indexing.Indexer
{
    public class PropertiesIndexer : BaseIndexer
    {
        public override string NotifyMessage => "Build Properties Index";

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
                new PropertyFrequencyIndexer(inputFilename),
                new PropertyDomainIndexer(inputFilename),
                new PropertyRangeIndexer(inputFilename)
            };
        }

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityP();
        }
    }
}