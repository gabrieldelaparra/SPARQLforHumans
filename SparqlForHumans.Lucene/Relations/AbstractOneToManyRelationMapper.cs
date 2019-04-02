using System.Collections.Generic;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Relations
{
    public abstract class AbstractOneToManyRelationMapper<T1, T2>
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();
        public int NotifyTicks { get; } = 100000;
        public abstract string NotifyMessage { get; set; }
        private int nodeCount = 0;

        public virtual Dictionary<T1, T2[]> GetRelationDictionary(string inputFilename)
        {
            var lines = FileHelper.GetInputLines(inputFilename);
            var subjectGroups = lines.GroupBySubject();

            return GetRelationDictionary(subjectGroups);
        }

        public virtual Dictionary<T1, T2[]> GetRelationDictionary(IEnumerable<SubjectGroup> subjectGroups)
        {
            var dictionary = new Dictionary<T1, List<T2>>();
            foreach (var subjectGroup in subjectGroups)
            {
                if (nodeCount % NotifyTicks == 0)
                    Logger.Info($"{NotifyMessage}, Entity Group: {nodeCount:N0}");
                
                AddToDictionary(dictionary, subjectGroup);
                
                nodeCount++;
            }

            Logger.Info($"{NotifyMessage}, Entity Group: {nodeCount:N0}");
            return dictionary.ToArrayDictionary();
        }

        internal abstract void AddToDictionary(Dictionary<T1, List<T2>> dictionary, SubjectGroup subjectGroup);

        //TODO: Check if required.
        //public static void AddDomainTypesToIndex(Directory propertiesIndexDirectory,
        //    Dictionary<int, int[]> propertyDomainTypes)
        //{
        //    var indexConfig = IndexConfiguration.CreateKeywordIndexWriterConfig();

        //    using (var writer = new IndexWriter(propertiesIndexDirectory, indexConfig))
        //    {
        //        foreach (var propertyDomain in propertyDomainTypes)
        //        {
        //            var propertyId = $"{WikidataDump.EntityPrefix}{propertyDomain.Key}";

        //            // Get the required document.
        //            // TODO: Check if it is better to iterate over all documents and update, or to search for it each time.
        //            var document = SingleDocumentQueries.QueryDocumentById(propertyId, propertiesIndexDirectory);

        //            // TODO: Recently removed the null check/continue. What if there is null doc?
        //            document?.AddDomainTypesToIndexDocument(propertyDomain);

        //            writer.UpdateDocument(new Term(Labels.Id.ToString(), propertyId), document);
        //        }
        //    }
        //}

        //TODO: Check if required.
        //private static void AddDomainTypesToIndexDocument(this Document document,
        //    KeyValuePair<int, int[]> propertyDomain)
        //{
        //    foreach (var domainType in propertyDomain.Value)
        //    {
        //        var domainId = $"{WikidataDump.EntityPrefix}{domainType}";
        //        var field = new TextField(Labels.DomainType.ToString(), domainId, Field.Store.YES);
        //        document.Add(field);
        //    }
        //}
    }
}
