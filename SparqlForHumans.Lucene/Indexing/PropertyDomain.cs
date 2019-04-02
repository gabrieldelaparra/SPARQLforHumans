using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Queries;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class PropertyDomain
    {
        /// <summary>
        ///     Given the following data:
        ///     ```
        ///     ...
        ///     Q76 -> P31 (Type) -> Q5
        ///     Q76 -> P27 -> Qxx
        ///     Q76 -> P555 -> Qxx
        ///     ...
        ///     Q298 -> P31 -> Q17
        ///     Q298 -> P555 -> Qxx
        ///     Q298 -> P777 -> Qxx
        ///     ...
        ///     ```
        ///     Returns the following domain:
        ///     P27: Domain Q5
        ///     P555: Domain Q5, Q17
        ///     P777: Domain Q17
        ///     Translated to the following KeyValue Pairs:
        ///     Key: 27; Values[]: 5
        ///     Key: 555; Values[]: 5, 17
        ///     Key: 777; Values[]: 17
        /// </summary>
        public static Dictionary<int, int[]> GetPropertyDomainTypes(this IEnumerable<SubjectGroup> subjectGroups)
        {
            var dictionary = new Dictionary<int, List<int>>();
            foreach (var subjectGroup in subjectGroups)
                dictionary.AddPropertyDomainTypesForSubjectGroup(subjectGroup);

            return dictionary.ToArrayDictionary();
        }

        public static void AddDomainTypesToIndex(Directory propertiesIndexDirectory,
            Dictionary<int, int[]> propertyDomainTypes)
        {
            var indexConfig = IndexConfiguration.CreateKeywordIndexWriterConfig();

            using (var writer = new IndexWriter(propertiesIndexDirectory, indexConfig))
            {
                foreach (var propertyDomain in propertyDomainTypes)
                {
                    var propertyId = $"Q{propertyDomain.Key}";

                    // Get the required document.
                    // TODO: Check if it is better to iterate over all documents and update, or to search for it each time.
                    var document = SingleDocumentQueries.QueryDocumentById(propertyId, propertiesIndexDirectory);

                    // TODO: Recently removed the null check/continue. What if there is null doc?
                    document?.AddDomainTypesToIndexDocument(propertyDomain);

                    writer.UpdateDocument(new Term(Labels.Id.ToString(), propertyId), document);
                }
            }
        }

        private static void AddDomainTypesToIndexDocument(this Document document,
            KeyValuePair<int, int[]> propertyDomain)
        {
            foreach (var domainType in propertyDomain.Value)
            {
                var domainId = $"Q{domainType}";
                var field = new TextField(Labels.DomainType.ToString(), domainId, Field.Store.YES);
                document.Add(field);
            }
        }

        /// <summary>
        ///     Given a SubjectGroup, get only the property-predicates.
        ///     Slice into instanceOf and direct-properties.
        ///     For each directProperty-Id, add all the instanceOf-Id
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="subjectGroup"></param>
        private static void AddPropertyDomainTypesForSubjectGroup(this Dictionary<int, List<int>> dictionary,
            SubjectGroup subjectGroup)
        {
            //Hopefully they should be already filtered.
            var propertiesTriples = subjectGroup.FilterPropertyPredicatesOnly();

            var (instanceOfSlice, otherPropertiesSlice) = propertiesTriples.SliceBy(x => x.Predicate.IsInstanceOf());

            // InstanceOf Ids (Domain Types) and Properties
            var propertyIds = otherPropertiesSlice.Select(x => x.Predicate.GetIntId()).ToArray();
            var domainIds = instanceOfSlice.Select(x => x.Object.GetIntId()).ToArray();

            foreach (var propertyId in propertyIds)
                dictionary.AddSafe(propertyId, domainIds);
        }


        /// <summary>
        ///     Allow only triples that have property predicates.
        /// </summary>
        /// <param name="entityGroupTriples"></param>
        /// <returns></returns>
        private static IEnumerable<Triple> FilterPropertyPredicatesOnly(this IEnumerable<Triple> entityGroupTriples)
        {
            return entityGroupTriples.Where(x => x.Predicate.IsProperty());
        }
    }
}