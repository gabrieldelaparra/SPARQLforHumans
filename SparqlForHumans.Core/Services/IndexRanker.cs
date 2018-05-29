using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SparqlForHumans.Core.Properties;
using SparqlForHumans.Core.Utilities;
using VDS.RDF;

namespace SparqlForHumans.Core.Services
{
    public class IndexRanker
    {
        static string last;
        static List<int> outLinksList;
        static int read = 0;
        static int currentIndex = 0;

        public static void handleStatement(Triple s, Dictionary<string, int> map, int[][] graph)
        {
            var TICKS = 100000;

            var entityIRI = WikidataDump.EntityIRI;
            read++;
            if (read % TICKS == 0)
            {
                //System.err.println(read + " lines read...");

                //long allocatedMemory = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
                //long freeMemory = Runtime.getRuntime().maxMemory() - allocatedMemory;

                //System.err.println("Free memory: " + freeMemory);
            }

            var subject = s.Subject.ToSafeString();
            // FIRST LINE
            if (last == null)
            {
                last = subject;
                var name = last.ToString();
                name = name.Replace(entityIRI, "");
                if (map.ContainsKey(name))
                {
                    currentIndex = map.GetValueOrDefault(name);
                    outLinksList = new List<int>();
                }
                else
                {
                    outLinksList = null;
                }
            }
            // NEW SUBJECT
            if (!last.Equals(subject))
            {
                if (outLinksList != null && !outLinksList.Any())
                {
                    graph[currentIndex] = outLinksList.ToArray();
                }

                last = subject;
                var name = last;
                name = name.Replace(entityIRI, "");
                if (map.ContainsKey(name))
                {
                    currentIndex = map.GetValueOrDefault(name);
                    outLinksList = new List<int>();
                }
                else
                {
                    outLinksList = null;
                }
            }
            // PROPERTIES
            if (s.Object.IsLiteral()) return;

            var ntobject = s.Object.ToString();
            var value = ntobject.Replace(entityIRI, "");

            if (outLinksList == null || !map.ContainsKey(value)) return;

            var valueId = map.GetValueOrDefault(value);

            if (!outLinksList.Contains(valueId))
                outLinksList.Add(valueId);
        }

        private static double[] rankGraph(int[][] graph)
        {
            int ITERATIONS = 25;
            double D = 0.85d;
            int nodes = graph.Length;

            double[] oldRanks = new double[nodes];

            double initial = 1d / nodes;

            for (int i = 0; i < nodes; i++)
            {
                oldRanks[i] = initial;
            }

            double[] ranks = null;
            for (int i = 0; i < ITERATIONS; i++)
            {
                double noLinkRank = 0d;
                ranks = new double[nodes];

                for (int j = 0; j < nodes; j++)
                {
                    if (graph[j] != null)
                    {
                        int[] out1 = graph[j];
                        double share = oldRanks[j] * D / out1.Length;
                        foreach (var o in out1)
                        {
                            ranks[o] += share;
                        }
                    }
                    else
                    {
                        noLinkRank += oldRanks[j];
                    }
                }

                double shareNoLink = (noLinkRank * D) / nodes;

                double shareMinusD = (1d - D) / nodes;

                double weakRank = shareNoLink + shareMinusD;

                double sum = 0d;
                double e = 0d;

                for (int k = 0; k < nodes; k++)
                {
                    ranks[k] += weakRank;
                    sum += ranks[k];
                    e += Math.Abs(oldRanks[k] - ranks[k]);
                }

                Array.Copy(ranks, 0, oldRanks, 0, nodes);
            }

            return ranks;
        }

        public static void Rank(Directory luceneIndexDirectory, string inputTriples, string ouputPath)
        {
            using (var reader = IndexReader.Open(luceneIndexDirectory, false))
            {
                int graphLength = reader.MaxDoc;
                var map = new Dictionary<string, int>();

                for (int i = 0; i < graphLength; i++)
                {
                    Document doc = reader.Document(i);
                    string key = doc.Get(Labels.Id.ToString());
                    map.Add(key, i);
                }

                int[][] graph = new int[graphLength][];

                var lines = FileHelper.GetInputLines(inputTriples);

                foreach (var line in lines)
                {
                    var triple = line.GetTriple();
                    handleStatement(triple, map, graph);
                }

                double[] ranks = rankGraph(graph);
            }
        }

        // INIT INDEX READER
        //    IndexReader reader = DirectoryReader.Open(FSDirectory.Open(Paths.get(dataDirectory)));
        //int graphLength = reader.MaxDoc;
        //Map<String, Integer> map = new HashMap<>();
        //System.err.println("Graph size: " + graphLength);

        //// CREATE MAP TO TRANSLATE SUBJECT TO ID
        //System.err.println("Creating map...");
        //for (int i = 0; i < graphLength; i++)
        //{
        //    Document doc = reader.document(i);
        //    String key = doc.get(DataFields.SUBJECT.name());
        //    map.put(key, i);
        //}
        //System.err.println("Map created successfully!");

        //// CREATE GRAPH IN MEMORY
        //System.err.println("Creating graph in memory...");
        //System.err.println("This may take a while...");
        //int[][] graph = new int[graphLength][];

        //InputStream in = new FileInputStream(triplesFile);
        //if (triplesFile.endsWith(".gz"))
        //{
        //    System.err.println("Input file is gzipped.");
        //in = new GZIPInputStream(in);
        //}
        //Reader isr = new InputStreamReader(in, "UTF-8");

        //RDFParser parser = Rio.createParser(RDFFormat.NTRIPLES);
        //RankHandler handler = new RankHandler(graph, map);
        //parser.setRDFHandler(handler);

        //System.err.println("Parsing file...");
        //System.err.println("This may take a while...");
        //try
        //{
        //    parser.parse(isr, "");
        //}
        //catch (Exception e)
        //{
        //    throw new IOException();
        //}
        //finally
        //{
        //in.close();
        //}
        //handler.finish();
        //System.err.println("Graph loaded");

        //// RANK GRAPH
        //System.err.println("Ranking graph...");
        //double[] ranks = rankGraph(graph);
        //System.err.println("Ranking complete!");

        //// WRITING TO OUTPUT
        //OutputStream os = new FileOutputStream(output);
        //os = new GZIPOutputStream(os);
        //PrintWriter pw = new PrintWriter(new OutputStreamWriter(new BufferedOutputStream(os)));
        //System.err.println("Writing ranks to " + output);

        //int written;
        //for (written = 0; written < ranks.length; written++)
        //{
        //    pw.println(written + "\t" + ranks[written]);
        //}
        //System.err.println("Finished writing ranks! Wrote " + written + " ranks.");

        //pw.close();
    }
}