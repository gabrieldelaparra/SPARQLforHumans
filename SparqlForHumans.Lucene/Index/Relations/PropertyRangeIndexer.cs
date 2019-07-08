using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index.Relations
{
    public class PropertyRangeIndexer : BaseOneToManyRelationMapper<int, int>, IFieldIndexer<StringField>
    {
        public PropertyRangeIndexer(string inputFilename) : base(inputFilename)
        {
        }

        public PropertyRangeIndexer(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
        {
        }

        public override string NotifyMessage => "Building <Property, RangeIds[]> Dictionary";

        public override Dictionary<int, int[]> BuildIndex(IEnumerable<SubjectGroup> subjectGroups)
        {
            var propertyObjectIdsDictionary = new Dictionary<int, List<int>>();
            var subjectIdTypeIdsDictionary = new Dictionary<int, List<int>>();
            var propertyRangeDictionary = new Dictionary<int, List<int>>();

            foreach (var subjectGroup in subjectGroups)
            {
                if (!subjectGroup.IsEntityQ()) continue;

                var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();
                var (instanceOfSlice, otherPropertiesSlice) =
                    propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

                foreach (var triple in otherPropertiesSlice)
                    propertyObjectIdsDictionary.AddSafe(triple.Predicate.GetIntId(), triple.Object.GetIntId());

                foreach (var triple in instanceOfSlice)
                    subjectIdTypeIdsDictionary.AddSafe(subjectGroup.IntId, triple.Object.GetIntId());
            }

            foreach (var pair in propertyObjectIdsDictionary)
            foreach (var objectId in pair.Value)
            {
                if (!subjectIdTypeIdsDictionary.ContainsKey(objectId)) continue;

                var objectTypes = subjectIdTypeIdsDictionary[objectId];
                propertyRangeDictionary.AddSafe(pair.Key, objectTypes);
            }

            return propertyRangeDictionary.ToArrayDictionary();
        }

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            throw new NotImplementedException();
        }

        public string FieldName => Labels.Range.ToString();
        public double Boost { get; set; }

        public IReadOnlyList<StringField> GetField(SubjectGroup tripleGroup)
        {
            return RelationIndex.ContainsKey(tripleGroup.Id.ToNumbers())
                ? RelationIndex[tripleGroup.Id.ToNumbers()]
                    .Select(x => new StringField(FieldName, x.ToString(), Field.Store.YES)).ToList()
                : new List<StringField>();
        }
    }
}