using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SparqlForHumans.Core.Models;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using Version = Lucene.Net.Util.Version;

namespace SparqlForHumans.Core.Services
{
    public static class QueryService
    {
        public static Entity AddProperties(this Entity entity, Directory luceneIndexDirectory)
        {
            var propertiesIds = entity.Properties.Select(x => x.Id);
            var properties = QueryEntityByIds(propertiesIds, luceneIndexDirectory);

            for (var i = 0; i < entity.Properties.Count(); i++)
            {
                var property = entity.Properties.ElementAt(i);
                var prop = properties.FirstOrDefault(x => x.Id.Equals(property.Id));
                property.Label = prop.Label;
            }

            return entity;
        }

        public static IEnumerable<Entity> QueryEntityByIds(IEnumerable<string> searchIds, Directory luceneIndexDirectory)
        {
            var entities = new List<Entity>();

            // NotEmpty Validation
            if (searchIds == null)
                return entities;

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new KeywordAnalyzer())
            {
                foreach (var searchText in searchIds)
                {
                    entities.Add(searchEntityById(searchText, queryAnalyzer, searcher));
                }
                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return entities;
        }

        public static Entity QueryEntityById(string searchId, Directory luceneIndexDirectory)
        {
            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchId))
                return null;

            var entity = new Entity();

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new KeywordAnalyzer())
            {
                entity = searchEntityById(searchId, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return entity;
        }

        private static Entity searchEntityById(string searchId, Analyzer queryAnalyzer, Searcher searcher)
        {
            var parser = new QueryParser(Version.LUCENE_30,Labels.Id.ToString(),queryAnalyzer);

            return searchEntity(searchId, searcher, parser);
        }

        public static Entity QueryEntityByLabel(string searchText, Directory luceneIndexDirectory)
        {
            if (string.IsNullOrEmpty(searchText))
                return null;

            searchText = PrepareSearchTerm(searchText);

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
                return null;

            var entity = new Entity();

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                entity = searchEntity(searchText, queryAnalyzer, searcher);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return entity;
        }

        /// <summary>
        /// Uses the default Lucene Index to query.
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static IEnumerable<Entity> QueryEntitiesByLabel(string searchText)
        {
            return QueryEntitiesByLabel(searchText, LuceneIndexExtensions.EntitiesIndexDirectory);
        }

        public static IEnumerable<Entity> QueryEntitiesByLabel(string searchText, Directory luceneIndexDirectory)
        {
            if (string.IsNullOrEmpty(searchText))
                return new List<Entity>();

            searchText = PrepareSearchTerm(searchText);
            const int resultsLimit = 20;

            var list = new List<Entity>();

            // NotEmpty Validation
            if (string.IsNullOrEmpty(searchText.Replace("*", "").Replace("?", "")))
                return list;

            using (var searcher = new IndexSearcher(luceneIndexDirectory, true))
            using (var queryAnalyzer = new StandardAnalyzer(Version.LUCENE_30))
            {
                list = searchEntities(searchText, queryAnalyzer, searcher, resultsLimit);

                queryAnalyzer.Close();
                searcher.Dispose();
            }
            return list;
        }

        private static Entity searchEntity(string searchText, Analyzer queryAnalyzer, Searcher searcher)
        {
            QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                new[] { Labels.Id.ToString(), Labels.Label.ToString(), Labels.AltLabel.ToString() },
                queryAnalyzer);

            return searchEntity(searchText, searcher, parser);
        }

        //TODO: Test Search Alt-Label
        //TODO: Test Search by Id
        //TODO: UI When searching by Person shows Human but can show Person and Alt-Labels as options
        //TODO: Some instances have more than one InstanceOf
        private static List<Entity> searchEntities(string searchText, Analyzer queryAnalyzer, Searcher searcher,
            int resultsLimit)
        {
            QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30,
                new[] { Labels.Id.ToString(), Labels.Label.ToString(), Labels.AltLabel.ToString() },
                queryAnalyzer);

            return searchEntities(searchText, searcher, resultsLimit, parser);
        }

        //TODO: Create Entity Interface
        //TODO: Refactor MapEntity to be a class object that can be exchanged to create different mappings.
        private static Entity searchEntity(string searchText, Searcher searcher, QueryParser parser)
        {
            var field = new SortField(Labels.Rank.ToString(), SortField.FLOAT);
            var sort = new Sort(field);

            var query = ParseQuery(searchText, parser);
            var hit = searcher.Search(query, null, 1, sort).ScoreDocs;

            if (hit == null || hit.Length.Equals(0))
                return null;

            return searcher.Doc(hit.FirstOrDefault().Doc).MapEntity();
        }

        private static List<Entity> searchEntities(string searchText, Searcher searcher, int resultsLimit, QueryParser parser)
        {
            var sort = new Sort(new SortField(Labels.Rank.ToString(), SortField.DOUBLE, true));

            var query = ParseQuery(searchText, parser);
            var hits = searcher.Search(query, null, resultsLimit, sort).ScoreDocs;
            //var hits = searcher.Search(query, null, resultsLimit).ScoreDocs;

            var entityList = new List<Entity>();
            foreach (var hit in hits)
            {
                var doc = searcher.Doc(hit.Doc);
                entityList.Add(doc.MapEntity());
            }

            return entityList;
        }

        public static string PrepareSearchTerm(string input)
        {
            var terms = input.Trim()
                .Replace("-", " ")
                .Split(' ')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Trim() + "*");

            return string.Join(" ", terms);
        }

        private static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }

            return query;
        }
    }
}