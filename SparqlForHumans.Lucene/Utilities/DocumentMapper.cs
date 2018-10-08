using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Models;

namespace SparqlForHumans.Lucene.Utilities
{
    public static class DocumentMapper
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


        //private static readonly Dictionary<string, string> typeLabels = new Dictionary<string, string>();
        //private static readonly Dictionary<string, string> propertyLabels = new Dictionary<string, string>();

        //public static string GetTypeLabel(string typeCode, Directory luceneIndexDirectory)
        //{
        //    if (typeCode == null) return string.Empty;

        //    if (typeLabels.ContainsKey(typeCode)) return typeLabels.FirstOrDefault(x => x.Key.Equals(typeCode)).Value;

        //    var label = GetLabelFromIndex(typeCode, luceneIndexDirectory);
        //    if (!string.IsNullOrWhiteSpace(label))
        //        typeLabels.Add(typeCode, label);

        //    return label;
        //}

        //public static string GetPropertyFromIndex(string propertyCode, Directory luceneIndexDirectory)
        //{
        //    if (propertyCode == null) return string.Empty;

        //    if (propertyLabels.ContainsKey(propertyCode))
        //        return propertyLabels.FirstOrDefault(x => x.Key.Equals(propertyCode)).Value;

        //    var label = GetLabelFromIndex(propertyCode, luceneIndexDirectory);

        //    if (!string.IsNullOrWhiteSpace(label))
        //        propertyLabels.Add(propertyCode, label);

        //    return label;
        //}

        //private static Analyzer analyzer;

        //private static IEnumerable<Entity> MapLuceneDocumentToData(IEnumerable<Document> documents)
        //{
        //    foreach (var doc in documents)
        //        yield return MapEntity(doc);
        //}

        //public static string GetLabelFromReader(this IndexSearcher indexSearcher, string entityName)
        //{
        //    var resultLimit = 1;
        //    var searchField = Labels.Id.ToString();

        //    // NotEmpty Validation
        //    if (string.IsNullOrEmpty(entityName))
        //        return string.Empty;

        //    var analyzer = new KeywordAnalyzer();
        //    var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);

        //    var query = QueryService.ParseQuery(entityName, parser);

        //    var hits = indexSearcher.Search(query, null, resultLimit).ScoreDocs;
        //    var result = string.Empty;

        //    if (hits.Any())
        //    {
        //        var doc = indexSearcher.Doc(hits.FirstOrDefault().Doc);
        //        result = doc.GetValue(Labels.Label);
        //    }
        //    analyzer.Close();
        //    return result;
        //}

        //public static string GetLabelFromIndex(string name, Directory luceneIndexDirectory)
        //{
        //    var resultLimit = 1;
        //    var searchField = Labels.Id.ToString();

        //    // NotEmpty Validation
        //    if (string.IsNullOrEmpty(name))
        //        return string.Empty;

        //    using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
        //    {
        //        //Plain Keyword Analyzer:
        //        analyzer = new KeywordAnalyzer();

        //        var parser = new QueryParser(Version.LUCENE_30, searchField, analyzer);

        //        var query = QueryService.ParseQuery(name, parser);
        //        var hits = searcher.Search(query, null, resultLimit).ScoreDocs;

        //        var result = string.Empty;

        //        if (hits.Any())
        //        {
        //            var doc = searcher.Doc(hits.FirstOrDefault().Doc);
        //            result = doc.GetValue(Labels.Label);
        //        }

        //        analyzer.Close();
        //        searcher.Dispose();

        //        return result;
        //    }
        //}

        //public static IEnumerable<Property> GetPropertiesFromIndex(this Document entityDocument, Directory lucenePropertiesIndexDirectory)
        //{
        //    var list = new List<Property>();

        //    foreach (var item in entityDocument.GetValues(Labels.Property.ToString()))
        //    {
        //        var propertyLabel = GetPropertyFromIndex(item, lucenePropertiesIndexDirectory);
        //        list.Add(new Property
        //        {
        //            Id = item,
        //            Label = propertyLabel,
        //            Value = string.Empty
        //        });
        //    }

        //    return list;
        //}

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