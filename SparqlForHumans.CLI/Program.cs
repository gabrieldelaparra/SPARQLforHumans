using SparqlForHumans.Core.Utilities;
using System;
using System.IO;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SparqlForHumans.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Options.InternUris = false;
            //Read GZip File
            var lines = GZipHandler.ReadGZip(@"C:\Users\admin\Desktop\DCC\latest-truthy.nt-gz\latest-truthy.nt.gz");
            var notifyTicks = 100000;
            var readCount = 0;
            var writeCount = 0;

            var entityIRI = "http://www.wikidata.org/entity/";
            var propertyIRI = "http://www.wikidata.org/prop/direct/";

            var labelIRI = "http://www.w3.org/2000/01/rdf-schema#label";
            var prefLabel = "http://www.w3.org/2004/02/skos/core#prefLabel";
            var nameIRI = "http://schema.org/name";
            var alt_labelIRI = "http://www.w3.org/2004/02/skos/core#altLabel";

            var descriptionIRI = "http://schema.org/description";

            var instanceOf = "P31";
            var image = "P18";

            var entityPrefix = "Q";

            //Foreach line in the Triples file:
            // - Parse the RDF triple
            // - For the following cases, skip:
            //      - If Subject Q-Code > 2mill
            //      - If Object Q-Code > 2mill
            //      - If Language != EN
            // Else, Add the triple to a new NT File

            using (var fileStream = new FileStream(@"C:\Users\admin\Desktop\DCC\SparqlForHumans\filtered-triples.nt", FileMode.Create))
            using (var filteredWriter = new StreamWriter(fileStream))
                foreach (var item in lines)
                {
                    readCount++;
                    if (readCount % notifyTicks == 0)
                    {
                        Console.WriteLine($"{readCount} lines read, {writeCount} written");
                        //break;
                    }
                    try
                    {
                        var g = new Graph();
                        StringParser.Parse(g, item);
                        var statement = g.Triples.Last();

                        if (statement.Subject.NodeType != NodeType.Uri) continue;

                        var ntSubject = (UriNode)statement.Subject;
                        var ntPredicate = statement.Predicate;
                        var ntObject = statement.Object;

                        //Condition: Subject is not Entity: Skip
                        if (!ntSubject.ToSafeString().Contains(entityIRI)) continue;

                        //Condition: Subject is Entity and Q > 2.000.000: Skip
                        if (ntSubject.Uri.Segments.Last().Contains(entityPrefix))
                        {
                            var index = ntSubject.Uri.Segments.Last().Replace(entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Entity and Q > 2.000.000: Skip
                        if (ntObject.NodeType == NodeType.Uri && ((UriNode)ntObject).Uri.Segments.Count() > 0 && ((UriNode)ntObject).Uri.Segments.Last().Contains(entityPrefix))
                        {
                            var index = ((UriNode)ntObject).Uri.Segments.Last().Replace(entityPrefix, string.Empty);
                            int.TryParse(index, out int indexInt);
                            if (indexInt > 2000000) continue;
                        }

                        //Condition: Object is Literal: Filter @en only
                        if (ntObject.NodeType == NodeType.Literal)
                            if (!((LiteralNode)ntObject).Language.Equals("en")) continue;

                        filteredWriter.WriteLine(item);
                        writeCount++;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Error: {readCount}, {item}");
                        Console.WriteLine(e.Message);
                    }
                    //Console.WriteLine($"item: {item}");
                    //Console.WriteLine($"triple:{Environment.NewLine}s:{statement.Subject}{Environment.NewLine}p:{statement.Predicate}{Environment.NewLine}o:{statement.Object}");
                }
            Console.WriteLine($"{readCount} lines read, {writeCount} written");
            Console.ReadLine();
        }
    }
}
