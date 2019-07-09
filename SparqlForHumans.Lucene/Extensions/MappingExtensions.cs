using Lucene.Net.Documents;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Extensions
{
    public static class MappingExtensions
    {
        public static void AddProperties(this List<Entity> entities, string indexPath)
        {
            var propertiesIds = entities.SelectMany(x => x.Properties).Select(x => x.Id).Distinct();
            var properties = new BatchIdQuery(indexPath, propertiesIds).GetDocuments().ToProperties();

            foreach (var entity in entities)
            {
                foreach (var property in entity.Properties)
                {
                    var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                    property.Label = prop.Label;
                }
            }
        }

        public static void AddProperties(this Entity entity, string indexPath)
        {
            AddProperties(new List<Entity> { entity }, indexPath);
        }

        public static void MapRank(this IHasRank<double> element, Document document)
        {
            element.Rank = document.GetValue(Labels.Rank).ToDouble();
        }

        public static void MapIsType(this IHasIsType element, Document document)
        {
            element.IsType = document.GetValue(Labels.IsTypeEntity).ToBool();
        }

        public static void MapId(this IHasId element, Document document)
        {
            element.Id = document.GetValue(Labels.Id);
        }

        public static void MapLabel(this IHasLabel element, Document document)
        {
            element.Label = document.GetValue(Labels.Label);
        }

        public static void MapInstanceOf(this Entity entity, Document doc)
        {
            entity.InstanceOf = doc.GetValues(Labels.InstanceOf);
        }

        public static void MapSubClass(this Entity entity, Document doc)
        {
            entity.SubClass = doc.GetValues(Labels.SubClass);
        }

        public static void MapBaseProperties(this Entity entity, Document document)
        {
            entity.Properties = document.ParseProperties().ToList();
        }

        public static List<Entity> ToEntities(this IReadOnlyList<Document> documents)
        {
            return documents?.Select(MapEntity).ToList();
        }

        public static List<Property> ToProperties(this IReadOnlyList<Document> documents)
        {
            return documents?.Select(MapProperty).ToList();
        }
        public static Entity MapEntity(this Document document)
        {
            var entity = new Entity();

            entity.MapId(document);
            entity.MapLabel(document);
            entity.MapAltLabels(document);
            entity.MapDescription(document);
            entity.MapInstanceOf(document);
            entity.MapSubClass(document);
            entity.MapRank(document);
            entity.MapIsType(document);
            entity.MapBaseProperties(document);

            return entity;
        }

        public static void MapRank(this IHasRank<int> element, Document document)
        {
            element.Rank = document.GetValue(Labels.Rank).ToInt();
        }

        public static void MapDescription(this IHasDescription element, Document document)
        {
            element.Description = document.GetValue(Labels.Description);
        }

        public static void MapAltLabels(this IHasAltLabel element, Document document)
        {
            element.AltLabels = document.GetValues(Labels.AltLabel);
        }

        public static void MapDomainTypes(this Property property, Document document)
        {
            property.DomainTypes = document.GetValues(Labels.DomainType);
        }

        public static Property MapProperty(this Document document)
        {
            var property = new Property();

            property.MapId(document);
            property.MapLabel(document);
            property.MapAltLabels(document);
            property.MapDescription(document);
            property.MapRank(document);
            property.MapDomainTypes(document);

            return property;
        }

        private static Property ParseProperty(string propertyId)
        {
            return new Property
            {
                Id = propertyId,
                Value = string.Empty,
                //TODO: Fix this somehow :D
                Label = string.Empty
            };
        }

        private static IEnumerable<Property> ParseProperties(this Document doc)
        {
            foreach (var item in doc.GetValues(Labels.Property))
            {
                yield return ParseProperty(item);
            }
        }
    }
}