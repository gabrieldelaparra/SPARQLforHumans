using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Relations;
using SparqlForHumans.Models.LuceneIndex;
using SparqlForHumans.RDF.Extensions;
using SparqlForHumans.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Lucene.Indexing
{
    public static class EntitiesIndex
    {
        private static readonly NLog.Logger Logger = SparqlForHumans.Logger.Logger.Init();

        public static int NotifyTicks { get; } = 100000;

        public static void CreateEntitiesIndex(string inputTriplesFilename, bool addBoosts = false)
        {
            using (var entitiesDirectory =
                FSDirectory.Open(LuceneIndexExtensions.EntityIndexPath.GetOrCreateDirectory()))
            {
                CreateEntitiesIndex(inputTriplesFilename, entitiesDirectory, addBoosts);
            }
        }

        //TODO: No 'prefLabel' in the current index:
        //TODO: No 'name' in the current index:
        /// EntitiesIndex
        /// Include Subjects only if Id starts with Q;
        /// Rank with boosts;
        /// Entities have properties, not sure if properties have properties;
        public static void CreateEntitiesIndex(string inputTriplesFilename, Directory outputDirectory,
            bool addBoosts = false)
        {
            long readCount = 1;

            // Read All lines in the file (IEnumerable, yield)
            // And group them by QCode.
            var lines = FileHelper.GetInputLines(inputTriplesFilename);
            var entityGroups = lines.GroupBySubject().Where(x => x.IsEntityQ());

            // Run PageRank (Document Boosts). Read all file: +2
            var entityPageRankDictionary = new Dictionary<int, double>();
            if (addBoosts)
                entityPageRankDictionary = EntityPageRank.BuildPageRank(inputTriplesFilename);

            // Get Entity and Types Dictionary. Read all file: +1
            var typeEntitiesDictionary = new TypeToEntitiesRelationMapper(entityGroups).RelationIndex;

            Logger.Info("Building Index");
            var indexConfig = IndexConfiguration.CreateStandardIndexWriterConfig();

            using (var writer = new IndexWriter(outputDirectory, indexConfig))
            {
                //Excludes Properties, will only add entities.
                foreach (var group in entityGroups)
                {
                    if (readCount % NotifyTicks == 0)
                        Logger.Info($"Build Entity Index, Group: {readCount:N0}");

                    var rdfIndexEntity = group.ToIndexEntity();

                    if (entityPageRankDictionary.ContainsKey(rdfIndexEntity.Id.ToNumbers()))
                        rdfIndexEntity.Rank = entityPageRankDictionary[rdfIndexEntity.Id.ToNumbers()];
                    if (typeEntitiesDictionary.ContainsKey(rdfIndexEntity.Id.ToNumbers()))
                        rdfIndexEntity.IsType = true;

                    writer.AddEntityDocument(rdfIndexEntity);

                    readCount++;
                }

                Logger.Info($"Build Entity Index, Group: {readCount:N0}");
            }
        }


        public static IEnumerable<Field> ParsePredicate(INode ntPredicate, INode ntObject)
        {
            var fields = new List<Field>();
            // On the existing Subject
            // If the predicate is a Property, add the property to a list of Properties and link it to the entity.
            // Else, (predicate not a property: Labels, Alt-Labels, Description, etc.)
            //  If the object is not a literal value, continue;
            // Otherwise, add the value to the index on each case.
            switch (ntPredicate.GetPredicateType())
            {
                case RDFExtensions.PredicateType.Property:
                    fields.AddRange(ParsePropertyPredicate(ntPredicate, ntObject));
                    break;
                case RDFExtensions.PredicateType.Label:
                    fields.Add(new TextField(Labels.Label.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES));
                    break;
                case RDFExtensions.PredicateType.Description:
                    fields.Add(new TextField(Labels.Description.ToString(),
                        ntObject.GetLiteralValue(), Field.Store.YES));
                    break;
                case RDFExtensions.PredicateType.AltLabel:
                    fields.Add(new TextField(Labels.AltLabel.ToString(), ntObject.GetLiteralValue(),
                        Field.Store.YES));
                    break;
                default:
                case RDFExtensions.PredicateType.Other:
                    break;
            }

            return fields;
        }

        /// <summary>
        ///     Used on creating a PropertyIndex.
        ///     Line example:
        ///     - P333 P31 P444.
        ///     - P333 is the subject.
        ///     - P31 is InstanceOf. (Could be a EntityDirected Predicate)
        ///     - P444 is the Type (InstanceOf) of P333.
        ///     Called for each property of a Property Subject.
        ///     Takes a Predicate, Object and a Property (Subject) Document.
        ///     Adds a new Field to the Document: InstanceOf, EntityDirected (With separators, not sure why).
        /// </summary>
        /// <param name="ntPredicate"></param>
        /// <param name="ntObject"></param>
        /// <param name="luceneDocument"></param>
        public static IEnumerable<Field> ParsePropertyPredicate(INode ntPredicate, INode ntObject)
        {
            var fields = new List<Field>();
            var propertyCode = ntPredicate.GetId();

            //Only Accept Q-Entities Property's Predicates
            if (!ntObject.IsEntityQ())
                return new List<Field>();

            //Stores the P-Id of the PropertyPredicate (of the PropertyEntity).
            fields.Add(new StringField(Labels.Property.ToString(), propertyCode, Field.Store.YES));

            switch (RDFExtensions.GetPropertyType(ntPredicate, ntObject))
            {
                //PropertyPredicate is InstanceOf another type of Property:
                case RDFExtensions.PropertyType.InstanceOf:
                    fields.Add(new StringField(Labels.InstanceOf.ToString(), ntObject.GetId(), Field.Store.YES));
                    break;

                //PropertyPredicate points to another Subject, most properties fall in here.
                //Properties are stored like
                //case RDFExtensions.PropertyType.EntityDirected:
                //    var propertyAndValue = propertyCode + WikidataDump.PropertyValueSeparator + ntObject.GetId();
                //    fields.Add(new StringField(Labels.PropertyAndValue.ToString(), propertyAndValue, Field.Store.YES));
                //    break;

                //Other cases, considered but not used.
                default:
                case RDFExtensions.PropertyType.LiteralDirected:
                case RDFExtensions.PropertyType.Other:
                    break;
            }

            return fields;
        }
    }
}