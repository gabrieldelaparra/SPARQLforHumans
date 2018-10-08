using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Extensions
{
    public static class MappingExtensions
    {
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
        public static IEnumerable<string> GetAltLabels(this Document doc)
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
            entity.Rank = document.GetValue(Labels.Rank);
            return entity;
        }

        public static Entity MapInstanceOf(this Entity entity, Document doc)
        {
            entity.InstanceOf = doc.GetValues(Labels.InstanceOf);
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
                .MapInstanceOf(document)
                .MapBaseProperties(document);

            return entity;
        }

        public static Property MapFrequency(this Property property, Document document)
        {
            property.Frequency = document.GetValue(Labels.Rank);
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