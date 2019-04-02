using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.RDFIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Models;
using VDS.RDF;

namespace SparqlForHumans.RDF.Extensions
{
    public static class SubjectGroupExtensions
    {
        public static IEnumerable<SubjectGroup> GroupBySubject(this IEnumerable<string> lines)
        {
            var subjectGroup = new SubjectGroup();
            var list = new List<string>();
            var last = string.Empty;

            foreach (var line in lines)
            {
                var entity = line.Split(WikidataDump.BlankSpaceChar).FirstOrDefault();

                //Base case: first value:
                if (last == string.Empty)
                {
                    list = new List<string>();
                    subjectGroup = new SubjectGroup(entity, list);
                    last = entity;
                }

                //Switch/Different of entity:
                // - Return list,
                // - Create new list,
                // - Assign last to current
                else if (last != entity)
                {
                    yield return subjectGroup;
                    list = new List<string>();
                    subjectGroup = new SubjectGroup(entity, list);
                    last = entity;
                }

                // Same entity
                list.Add(line);
            }

            yield return subjectGroup;
        }

        public static bool IsEntityQ(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith(WikidataDump.EntityPrefix);
        }

        public static bool IsEntityP(this SubjectGroup subjectGroup)
        {
            return subjectGroup.Id.StartsWith(WikidataDump.PropertyPrefix);
        }

        public static RDFIndexEntity ToIndexEntity(this SubjectGroup subjectGroup)
        {
            var entity = new RDFIndexEntity(subjectGroup.Id);

            foreach (var triple in subjectGroup)
                entity.ParseSubjectGroupTriple(triple);

            return entity;
        }

        // TODO: Add SubClass support
        private static void ParseSubjectGroupTriple(this RDFIndexEntity entity, Triple triple)
        {
            switch (triple.Predicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Property:
                    entity.ParseSubjectGroupProperties(triple);
                    break;
                case RDFExtensions.PredicateType.Label:
                    entity.Label = triple.Object.GetLiteralValue();
                    break;
                case RDFExtensions.PredicateType.Description:
                    entity.Description = triple.Object.GetLiteralValue();
                    break;
                case RDFExtensions.PredicateType.AltLabel:
                    entity.AltLabels.Add(triple.Object.GetLiteralValue());
                    break;
                default:
                case RDFExtensions.PredicateType.Other:
                    break;
            }
        }

        private static void ParseSubjectGroupProperties(this RDFIndexEntity entity, Triple triple)
        {
            var propertyCode = triple.Predicate.GetId();
            switch (RDFExtensions.GetPropertyType(triple.Predicate, triple.Object))
            {
                //PropertyPredicate is InstanceOf another type of Property:
                case RDFExtensions.PropertyType.InstanceOf:
                    entity.InstanceOf.Add(triple.Object.GetId());
                    break;

                //Other cases, considered but not used.
                default:
                case RDFExtensions.PropertyType.EntityDirected:
                case RDFExtensions.PropertyType.LiteralDirected:
                case RDFExtensions.PropertyType.Other:
                    entity.Properties.Add(propertyCode);
                    break;
            }
        }
    }
}