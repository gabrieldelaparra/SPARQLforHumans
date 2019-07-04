using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Extensions
{
    public static class MappingExtensions
    {
        public static void AddProperties(this List<Entity> entities)
        {
            using (var propertiesDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                var propertiesIds = entities.SelectMany(x=>x.Properties).Select(x => x.Id).Distinct();
                var properties = MultiDocumentQueries.QueryPropertiesByIds(propertiesIds, propertiesDirectory);

                //for (int i = 0; i < entities.Count(); i++)
                //{
                //    var entity = entities.ElementAt(i);
                //    for (int j = 0; j < entity.Properties.Count(); j++)
                //    {
                //        var property = entity.Properties.ElementAt(j);
                //        var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                //        property.Label = prop.Label;
                //    }
                //}
                foreach (var entity in entities)
                {
                    foreach (var property in entity.Properties)
                    {
                        var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                        property.Label = prop.Label;
                    }
                }
            }
        }

        public static Entity AddProperties(this Entity entity)
        {
            using (var propertiesDirectory =
                FSDirectory.Open(LuceneIndexExtensions.PropertyIndexPath.GetOrCreateDirectory()))
            {
                return AddProperties(entity, propertiesDirectory);
            }
        }

        public static Entity AddProperties(this Entity entity, Directory luceneDirectory)
        {
            var propertiesIds = entity.Properties.Select(x => x.Id);
            var properties = MultiDocumentQueries.QueryPropertiesByIds(propertiesIds, luceneDirectory);

            for (var i = 0; i < entity.Properties.Count(); i++)
            {
                var property = entity.Properties.ElementAt(i);
                var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                property.Label = prop.Label;
            }

            return entity;
        }

        public static ISubject MapBaseSubject(this Document document)
        {
            ISubject entity = new Subject
            {
                Id = document.GetValue(Labels.Id),
                Label = document.GetValue(Labels.Label)
            };
            return entity;
        }

        /// <summary>
        ///     Gets Alt-Label collection for a document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static IList<string> GetAltLabels(this Document doc)
        {
            var labels = doc.GetValues(Labels.Label);
            var altLabels = doc.GetValues(Labels.AltLabel);

            return altLabels.Length.Equals(0) ? labels : altLabels;
        }

        public static Entity MapAltLabels(this Entity entity, Document document)
        {
            entity.AltLabels = GetAltLabels(document);
            return entity;
        }

        public static Entity MapDescription(this Entity entity, Document document)
        {
            entity.Description = document.GetValue(Labels.Description);
            return entity;
        }

        public static Entity MapRank(this Entity entity, Document document)
        {
            entity.Rank = document.GetValue(Labels.Rank).ToDouble();
            return entity;
        }

        public static Entity MapIsType(this Entity entity, Document document)
        {
            entity.IsType = document.GetValue(Labels.IsTypeEntity).ToBool();
            return entity;
        }

        public static Entity MapInstanceOf(this Entity entity, Document doc)
        {
            entity.InstanceOf = doc.GetValues(Labels.InstanceOf);
            return entity;
        }

        public static Entity MapSubClass(this Entity entity, Document doc)
        {
            entity.SubClass = doc.GetValues(Labels.SubClass);
            return entity;
        }

        public static Entity MapBaseProperties(this Entity entity, Document document)
        {
            entity.Properties = document.ParseProperties().ToList();
            return entity;
        }

        public static Entity MapEntity(this Document document)
        {
            var entity = new Entity(document.MapBaseSubject())
                .MapAltLabels(document)
                .MapDescription(document)
                .MapRank(document)
                .MapIsType(document)
                .MapSubClass(document)
                .MapInstanceOf(document)
                .MapBaseProperties(document);

            return entity;
        }

        public static Property MapFrequency(this Property property, Document document)
        {
            property.Rank = document.GetValue(Labels.Rank).ToInt();
            return property;
        }

        public static Property MapDomainTypes(this Property property, Document document)
        {
            property.DomainTypes = document.GetValues(Labels.DomainType);
            return property;
        }

        public static Property MapProperty(this Document document)
        {
            var property = new Property(document.MapBaseSubject())
                .MapFrequency(document)
                .MapDomainTypes(document);

            return property;
        }

        private static Property ParseProperty(string indexProperty)
        {
            var propertyId = indexProperty;

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
                yield return ParseProperty(item);
        }
    }
}