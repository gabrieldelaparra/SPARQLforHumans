using Lucene.Net.Index;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;

namespace SparqlForHumans.Lucene.Index
{
    //public class PropertiesIndexer : BaseIndexer
    //{
    //    public PropertiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
    //    {
    //        RelationMappers = new List<IFieldIndexer<IIndexableField>>
    //        {
    //            new PropertyFrequencyIndexer(inputFilename),
    //            new PropertyDomainIndexer(inputFilename),
    //            new PropertyRangeIndexer(inputFilename)
    //        };

    //        FieldIndexers = new List<IFieldIndexer<IIndexableField>>
    //        {
    //            new IdIndexer(),
    //            new LabelIndexer(),
    //            new AltLabelIndexer(),
    //            new DescriptionIndexer()
    //        };
    //    }

    //    public override string NotifyMessage => "Build Properties Index";

    //    public override bool FilterGroups(SubjectGroup tripleGroup)
    //    {
    //        return tripleGroup.IsEntityP();
    //    }
    //}
}