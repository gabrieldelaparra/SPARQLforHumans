using System.Collections.Generic;
using Lucene.Net.Index;
using SparqlForHumans.Lucene.Indexing.BaseFields;
using SparqlForHumans.Lucene.Indexing.EntityFields;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public class EntitiesIndexer<TKey, TValue> : AbstractIndexer<IIndexableField, TKey, TValue>
    where TKey : struct
    where TValue : class
    {
        public EntitiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            IRelationMapper<Dictionary<int, int[]>> a = new TypeToEntitiesRelationMapper();
            IRelationMapper<Dictionary<int, int>> b = new PropertiesFrequencyRelationMapper();
            IRelationMapper<object> c = new TypeToEntitiesRelationMapper();
            //IRelationMapper<Dictionary<TKey, TValue>> d = new PropertiesFrequencyRelationMapper();

            //var basea = new List<IRelationMapper<Dictionary<int, int[]>>>()
            //{
            //    new TypeToEntitiesRelationMapper(),
            //};

            //var baseb = new List<IRelationMapper<Dictionary<int, int>>>()
            //{
            //    new PropertiesFrequencyRelationMapper(),
            //};

            //var basec = new List<IRelationMapper<Dictionary<TKey, int>>>()
            //{
            //    new PropertiesFrequencyRelationMapper(),
            //};

            //var baser = new List<IRelationMapper<object>>()
            //{
            //    new TypeToEntitiesRelationMapper().GetRelationDictionary(InputFilename),
            //    new PropertiesFrequencyRelationMapper().GetRelationDictionary(InputFilename),
            //};

            //foreach (var relationMapper in baser)
            //{
            //    var f = relationMapper.GetRelationDictionary("a");
            //}

            var mappers = new List<AbstractOneToManyRelationMapper<int, int>>()
            {
                new TypeToEntitiesRelationMapper(),
            };
            //var mappers = new List<IRelationMapper<Dictionary<int, int>>>()
            //{
            //    new TypeToEntitiesRelationMapper(),
            //};
            //RelationMappers = new List<IRelationMapper<IDictionary<TKey, TValue>>>()
            //{
            //    new TypeToEntitiesRelationMapper(),
            //};

            FieldIndexers = new List<ISubjectGroupIndexer<IIndexableField>>()
            {
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer(),
                new InstanceOfIndexer(),
                new SubClassIndexer(),
                new PropertiesIndexer(),
            };

        }

        //public sealed override IEnumerable<IRelationMapper<TDictionary>> RelationMappers { get; set; }
        public override IEnumerable<IRelationMapper<IDictionary<TKey, TValue>>> RelationMappers { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public sealed override IEnumerable<ISubjectGroupIndexer<IIndexableField>> FieldIndexers { get; set; }

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityQ();
        }
    }
}
