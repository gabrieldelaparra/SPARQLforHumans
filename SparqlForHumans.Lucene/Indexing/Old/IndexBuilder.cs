using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Models;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class IndexBuilder
    {
        /// <summary>
        ///     From an existing Entities Index, for each `entityId` get all the `propertyId` of that Entity.
        /// </summary>
        /// <param name="entitiesIndexDirectory"></param>
        /// <returns></returns>
        //public static Dictionary<int, int[]> CreateTypesAndPropertiesDictionary(
        //    Directory entitiesIndexDirectory)
        //{
        //    var dictionary = new Dictionary<int, List<int>>();

        //    using (var entityReader = DirectoryReader.Open(entitiesIndexDirectory))
        //    {
        //        var docCount = entityReader.NumDocs;
        //        for (var i = 0; i < docCount; i++)
        //        {
        //            var doc = entityReader.Document(i);

        //            var entity = new Entity(doc.MapBaseSubject())
        //                .MapInstanceOf(doc)
        //                .MapRank(doc)
        //                .MapBaseProperties(doc);

        //            foreach (var instanceOf in entity.InstanceOf)
        //                dictionary.AddSafe(instanceOf.ToInt(), entity.Properties.Select(x => x.Id.ToInt()));
        //        }
        //    }

        //    return dictionary.ToArrayDictionary();
        //}

        ///// <summary>
        /////     This method takes a document, all fields that are going to be added to that document and
        /////     a boost factor. That boost factor is only added to the `Label` and `AltLabel` fields.
        /////     Before, each `AltLabel` item was added as a new Field. Now all `AltLabel`s are concatenated with
        /////     `##` and a single boost is added to the `AltLabel` field. On Query/Map, `AltLabel`s are split.
        ///// </summary>
        ///// <param name="doc"></param>
        ///// <param name="fields"></param>
        ///// <param name="boost"></param>
        //public static void AddFields(Document doc, IEnumerable<Field> fields, double boost = 0)
        //{
        //    //AltLabels: Join with ## and add Boost.
        //    var altLabelValues = fields
        //        .Where(x => x.Name.Equals(Labels.AltLabel.ToString()))
        //        .Select(x => x.GetStringValue());

        //    var altLabelField = new TextField(Labels.AltLabel.ToString(),
        //        string.Join(" ## ", altLabelValues),
        //        Field.Store.YES)
        //    {
        //        Boost = (float) boost
        //    };

        //    doc.Add(altLabelField);

        //    //Labels: Set Boost
        //    foreach (var field in fields.Where(x => x.Name.Equals(Labels.Label.ToString())))
        //        field.Boost = (float) boost;

        //    //Non AltLabels
        //    fields = fields.Where(x => !x.Name.Equals(Labels.AltLabel.ToString()));
        //    foreach (var field in fields)
        //        doc.Add(field);
        //}

        
    }
}