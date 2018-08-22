﻿using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Properties;

namespace SparqlForHumans.Core.Utilities
{
    public static class DocumentMapper
    {
        public static Entity MapBaseEntity(this Document document)
        {
            return new Entity()
            {
                Id = document.GetValue(Labels.Id),
                Label = document.GetValue(Labels.Label),
            };
        }

        /// <summary>
        /// Gets Alt-Label collection for a document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetAltLabels(this Document doc)
        {
            var labels = doc.GetValues(Labels.Label);
            var altLabels = doc.GetValues(Labels.AltLabel);

            return altLabels.Length.Equals(0) ? labels : altLabels;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Entity GetAltLabels(this Entity entity, Document document)
        {
            entity.AltLabels = GetAltLabels(document);
            return entity;
        }

        public static Entity GetDescription(this Entity entity, Document document)
        {
            entity.Description = document.GetValue(Labels.Description);
            return entity;
        }



        public static Entity GetInstanceOf(this Entity entity, Document document)
        {
            entity.InstanceOf = document.GetValue(Labels.InstanceOf);
            return entity;
        }

        public static Entity GetBaseProperties(this Entity entity, Document document)
        {
            entity.Properties = document.ParsePropertiesAndValues().ToList();
            return entity;
        }

        public static Entity MapEntity(this Document document)
        {
            var entity = document.MapBaseEntity()
                                 .GetAltLabels(document)
                                 .GetDescription(document)
                                 .GetInstanceOf(document)
                                 .GetBaseProperties(document);

            return entity;
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

        public static Property ParsePropertyAndValue(string indexPropertyAndValue)
        {
            if (!indexPropertyAndValue.Contains(WikidataDump.PropertyValueSeparator))
                return null;

            var splitted = indexPropertyAndValue.Split(WikidataDump.PropertyValueSeparator);

            var propertyId = splitted[0];
            var propertyValue = splitted[1];

            return new Property()
            {
                Id = propertyId,
                Value = propertyValue,
                //TODO: Fix this somehow :D
                Label = string.Empty,
            };
        }

        public static IEnumerable<Property> ParsePropertiesAndValues(this Document doc)
        {
            foreach (var item in doc.GetValues(Labels.PropertyAndValue))
            {
                yield return ParsePropertyAndValue(item);
            }
        }


    }
}