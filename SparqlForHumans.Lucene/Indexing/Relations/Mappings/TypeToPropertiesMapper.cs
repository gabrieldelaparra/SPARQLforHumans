using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SparqlForHumans.Lucene.Indexing.Relations.Mappings.Base;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing.Relations.Mappings
{
    public class TypeToPropertiesMapper : BaseOneToManyRelationMapper<int, int>
    {
        public override string NotifyMessage { get;  } = "Building <Type, Properties[]> Dictionary";

        public TypeToPropertiesMapper(string inputFilename) : base(inputFilename)
        {
        }

        public TypeToPropertiesMapper(IEnumerable<SubjectGroup> subjectGroups) : base(subjectGroups)
        {
        }

        internal override void ParseTripleGroup(Dictionary<int, List<int>> dictionary, SubjectGroup subjectGroup)
        {
            //Hopefully they should be already filtered.
            var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();
            var instanceOfIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();

            foreach (var instanceOfId in instanceOfIds)
                dictionary.AddSafe(instanceOfId, propertyIds);
        }

        
    }
}
