﻿using Lucene.Net.Documents;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Index.Fields
{
    public class EntityPropertiesIndexer : BaseFieldIndexer<StringField>
    {
        public override string FieldName => Labels.Property.ToString();

        public override bool FilterValidTriples(Triple triple)
        {
            return triple.Predicate.GetPredicateType().Equals(PredicateType.Property)
                   && RDFExtensions.GetPropertyType(triple.Predicate)
                       .Equals(PropertyType.Other);
        }

        public override string SelectTripleValue(Triple triple)
        {
            return triple.Predicate.GetId();
        }

        public override IReadOnlyList<StringField> GetField(SubjectGroup tripleGroup)
        {
            var values = TriplesToValue(tripleGroup);
            return values.Any()
                ? values.Select(x => new StringField(FieldName, x, Field.Store.YES)).ToList()
                : new List<StringField>();
        }
    }
}