using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Logger;
using SparqlForHumans.Lucene.Index.Base;
using SparqlForHumans.Lucene.Index.Fields;
using SparqlForHumans.Lucene.Index.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.Models.Wikidata;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.RDF.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Index
{
    //public struct SimpleTriple
    //{
    //    public string Subject { get; set; }
    //    public string Predicate { get; set; }
    //    public string Object { get; set; }
    //}

    //public struct Subjects
    //{
    //    public string Id { get; set; }
    //    public IEnumerable<string> TripleLines { get; set; }
    //}

    //public static class WikidataDumpExtensions
    //{
    //    public static IEnumerable<Subjects> ToSubjects(this IEnumerable<string> lines)
    //    {
    //        var subjectGroup = new Subjects();
    //        var list = new List<string>();
    //        var last = string.Empty;

    //        foreach (var line in lines)
    //        {
    //            //Avoid non-entities, from start:
    //            if (!line.IsEntity()) continue;

    //            var entity = line.Split(Constants.BlankSpaceChar).FirstOrDefault();

    //            //Base case: first value:
    //            if (last == string.Empty)
    //            {
    //                list = new List<string>();
    //                subjectGroup = new Subjects()
    //                {
    //                    Id = entity,
    //                    TripleLines = list,
    //                };
    //                last = entity;
    //            }

    //            //Switch/Different of entity:
    //            // - Return existing list,
    //            // - Create new list,
    //            // - Assign last to current
    //            else if (last != entity)
    //            {
    //                yield return subjectGroup;
    //                list = new List<string>();
    //                subjectGroup = new Subjects()
    //                {
    //                    Id = entity,
    //                    TripleLines = list,
    //                };
    //                last = entity;
    //            }

    //            // Same entity
    //            list.Add(line);
    //        }

    //        yield return subjectGroup;
    //    }

    //    public static SimpleTriple ToSimpleTriple(this string line)
    //    {
    //        var subject = string.Empty;
    //        var predicate = string.Empty;
    //        var rest = string.Empty;

    //        var entitySpaceIndex = line.IndexOf(Constants.BlankSpaceChar);

    //        if (entitySpaceIndex < 0)
    //            throw new ArgumentException($"Invalid Triple: {line}");

    //        subject = line.Substring(0, entitySpaceIndex);

    //        var predicateAndObject = line.Substring(entitySpaceIndex + 1);

    //        var predicateSpaceIndex = predicateAndObject.IndexOf(Constants.BlankSpaceChar);

    //        if (predicateSpaceIndex < 0)
    //            throw new ArgumentException($"Invalid Triple: {line}");

    //        predicate = line.Substring(0, predicateSpaceIndex);

    //        rest = line.Substring(predicateSpaceIndex + 1);

    //        return new SimpleTriple()
    //        {
    //            Subject = subject,
    //            Predicate = predicate,
    //            Object = rest,
    //        };
    //    }

    //    public static bool IsEntity(this string entity)
    //    {
    //        return entity.StartsWith(Constants.EntityUriString);
    //    }

    //    public static bool IsEntityQ(this string entity)
    //    {
    //        return entity.StartsWith(Constants.EntityUriStringQ);
    //    }

    //    public static bool IsEntityP(this string entity)
    //    {
    //        return entity.StartsWith(Constants.EntityUriStringP);
    //    }

    //    public static bool IsDirectProperty(this string predicate)
    //    {
    //        return predicate.StartsWith(Constants.PropertyIRIString);
    //    }

    //    public static string RemoveEntityUri(this string entity)
    //    {
    //        return entity.Replace(Constants.EntityUriString, string.Empty);
    //    }
    //}
    //public class SimpleEntitiesIndexer : BaseNotifier
    //{
    //    public override string NotifyMessage => "Build Entities Index";

    //    public SimpleEntitiesIndexer(string inputFilename, string outputDirectory)
    //    {
    //        long readCount = 0;

    //        // Read All lines in the file (IEnumerable, yield)
    //        // And group them by QCode.
    //        var subjectGroups = FileHelper.GetInputLines(inputFilename).ToSubjects().Where(x => x.Id.IsEntityQ());

    //        var indexConfig = LuceneIndexDefaults.CreateStandardIndexWriterConfig();

    //        using (var indexDirectory = FSDirectory.Open(outputDirectory.GetOrCreateDirectory()))
    //        {
    //            using var writer = new IndexWriter(indexDirectory, indexConfig);
    //            foreach (var subjectGroup in subjectGroups)
    //            {
    //                var document = new Document();

    //                //foreach (var fieldIndexer in RelationMappers) {
    //                //    foreach (var field in fieldIndexer.GetField(subjectGroup)) {
    //                //        document.Add(field);
    //                //    }
    //                //}

    //                var boostField = document.Fields.FirstOrDefault(x => x.Name.Equals(Labels.Rank.ToString()));
    //                var boost = 0.0;
    //                if (boostField != null) boost = (double)boostField.GetDoubleValue();

    //                foreach (var fieldIndexer in FieldIndexers)
    //                {
    //                    fieldIndexer.Boost = boost;
    //                }

    //                foreach (var fieldIndexer in FieldIndexers)
    //                {
    //                    foreach (var field in fieldIndexer.GetField(subjectGroup))
    //                    {
    //                        document.Add(field);
    //                    }
    //                }

    //                LogProgress(readCount++);

    //                if (FilterGroups(subjectGroup)) writer.AddDocument(document);
    //            }
    //        }

    //        LogProgress(readCount, true);

    //    }
    //}

    public class EntitiesIndexer : BaseIndexer
    {
        public EntitiesIndexer(string inputFilename, string outputDirectory) : base(inputFilename, outputDirectory)
        {
            RelationMappers = new List<IFieldIndexer<IIndexableField>> {
                new EntityPageRankBoostIndexer(inputFilename)
            };

            //TODO: No 'prefLabel' in the current index:
            //TODO: No 'name' in the current index:
            FieldIndexers = new List<IFieldIndexer<IIndexableField>> {
                new IdIndexer(),
                new LabelIndexer(),
                new AltLabelIndexer(),
                new DescriptionIndexer(),
                new InstanceOfIndexer(),
                new EntityPropertiesIndexer(),
                new SubClassIndexer(),
                new ReverseEntityPropertiesIndexer(),
                //new ReverseInstanceOfIndexer(),
                new ReverseIsTypeIndexer()
            };
        }

        public override string NotifyMessage => "Build Entities Index";

        public override bool FilterGroups(SubjectGroup tripleGroup)
        {
            return tripleGroup.IsEntityQ();
        }
    }
}