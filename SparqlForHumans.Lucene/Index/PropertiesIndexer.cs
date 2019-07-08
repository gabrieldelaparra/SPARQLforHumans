using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;

namespace SparqlForHumans.Lucene.Index
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
                new PropertyFrequencyIndexer(inputFilename),
                new PropertyDomainIndexer(inputFilename),
                new PropertyRangeIndexer(inputFilename)
            };
        }

        public override string NotifyMessage => "Build Properties Index";

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityP();
        }
    }
}