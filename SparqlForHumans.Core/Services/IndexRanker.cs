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
    public class GraphNode
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public List<string> ConnectedNodes { get; set; } = new List<string>();
        public double Rank { get; set; }

        public GraphNode(string id, int index)
        {
            Id = id;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Id} - {ConnectedNodes.Count}";
        }
    }

    public static class IndexRanker
    {
        public static double GiveMeThreeDecimalsOnly(this double input)
        {
            return Math.Truncate(input * 1000) / 1000;
        }

        private static double pageRankAlpha = 0.85d;
        private static double noLinkRank = 0d;

        static string last;
        static List<int> outLinksList;
        static int read = 0;
        static int currentIndex = 0;

        public static IEnumerable<GraphNode> BuildNodesGraph(string triplesFilename)
        {
            var list = new List<GraphNode>();
            var lines = FileHelper.GetInputLines(triplesFilename);
            var groups = lines.GroupByEntities();

            var nodeCount = 0;

            foreach (var group in groups)
            {
                var subjectId = group.FirstOrDefault().GetTriple().Subject.GetId();
                var entityNode = new GraphNode(subjectId, nodeCount);

                foreach (var line in group)
                {
                    var (_, _, ntObject) = line.GetTripleAsTuple();

                    if (!ntObject.IsEntity())
                        continue;

                    var objectId = ntObject.GetId();

                    if (!entityNode.ConnectedNodes.Contains(objectId))
                        entityNode.ConnectedNodes.Add(objectId);
                }

                list.Add(entityNode);
                nodeCount++;
            }

            //Assign initial rank to all nodes;
            var initialRank = 1d / nodeCount;
            foreach (var graphNode in list)
            {
                graphNode.Rank = initialRank;
            }

            return list;
        }

        public static void CalculateRanks(IEnumerable<GraphNode> graphNodes, int iterations)
        {
            for (var i = 0; i < iterations; i++)
            {
                CalculateRanks(graphNodes);
            }
        }

        public static void CalculateRanks(IEnumerable<GraphNode> graphNodes)
        {
            //TODO: Check if this takes too much time for a large graph;
            var nodeCount = graphNodes.Count();

            var noLinkRank = 0d;
            var ranks = new double[nodeCount];

            foreach (var graphNode in graphNodes)
            {
                if (graphNode.ConnectedNodes.Any())
                {
                    var share = graphNode.Rank * pageRankAlpha / graphNode.ConnectedNodes.Count;
                    foreach (var connectedNode in graphNode.ConnectedNodes)
                    {
                        //TODO: This might take too much time. That's why it seems to be easier to work with indexes.
                        ranks[graphNodes.FirstOrDefault(x => x.Id.Equals(connectedNode)).Index] += share;
                    }
                }
                else
                {
                    noLinkRank += graphNode.Rank;
                }
            }
            var shareNoLink = (noLinkRank * pageRankAlpha) / nodeCount;
            var shareMinusD = (1d - pageRankAlpha) / nodeCount;
            var weakRank = shareNoLink + shareMinusD;

            foreach (var graphNode in graphNodes)
            {
                ranks[graphNode.Index] += weakRank;
                graphNode.Rank = ranks[graphNode.Index];
            }
        }

        private static double[] IteratePageRank(IEnumerable<GraphNode> graphNodes)
        {
            var nodeCount = graphNodes.Count();
            int[][] graph = new int[nodeCount][];
            var iterations = 25;

            var oldRanks = new double[nodeCount];

            var initialRank = 1d / nodeCount;

            for (var i = 0; i < nodeCount; i++)
            {
                oldRanks[i] = initialRank;
            }

            double[] ranks = null;
            for (var i = 0; i < iterations; i++)
            {
                var noLinkRank = 0d;
                ranks = new double[nodeCount];

                foreach (var graphNode in graphNodes)
                {
                    if (graphNode.ConnectedNodes.Any())
                    {
                        var share = graphNode.Rank * pageRankAlpha / graphNode.ConnectedNodes.Count;
                        foreach (var connectedNode in graphNode.ConnectedNodes)
                        {
                            //This will take too much time. That's why it seems to be easier to work with indexes.
                            graphNodes.FirstOrDefault(x => x.Id.Equals(connectedNode)).Rank += share;
                        }
                    }
                    else
                    {
                        noLinkRank += graphNode.Rank;
                    }
                }
                var _shareNoLink = (noLinkRank * pageRankAlpha) / nodeCount;
                var _shareMinusD = (1d - pageRankAlpha) / nodeCount;
                var _weakRank = _shareNoLink + _shareMinusD;

                var _sum = 0d;
                var _e = 0d;

                foreach (var graphNode in graphNodes)
                {
                    graphNode.Rank += _weakRank;
                    _sum += graphNode.Rank;
                    //_e += Math.Abs(oldRanks[k] - ranks[k]);
                }


                for (var j = 0; j < nodeCount; j++)
                {
                    if (graph[j] != null)
                    {
                        int[] out1 = graph[j];
                        var share = oldRanks[j] * pageRankAlpha / out1.Length;
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

                var shareNoLink = (noLinkRank * pageRankAlpha) / nodeCount;

                var shareMinusD = (1d - pageRankAlpha) / nodeCount;

                var weakRank = shareNoLink + shareMinusD;

                var sum = 0d;
                var e = 0d;

                for (var k = 0; k < nodeCount; k++)
                {
                    ranks[k] += weakRank;
                    sum += ranks[k];
                    e += Math.Abs(oldRanks[k] - ranks[k]);
                }

                Array.Copy(ranks, 0, oldRanks, 0, nodeCount);
            }

            return ranks;
        }

        /// <summary>
        /// Rank lee el indice y saca cuantos nodos son.
        /// Luego crea un diccionario(string, int).
        /// Para cada documento del indice, crea una llave, ejemplo: (Q1, 1) (Q2,2) -Sin Q3- (Q4,3)
        /// Luego lee nuevamente las tuplas desde el archivo de entrada y les da el handle.
        /// El Handle, toma cada triple, el diccionario, y el graph.
        /// Entiendo que Handle, debería modificar el graph.
        /// Luego corre el codigo de pagerank.
        /// Este codigo entrega (no está incluido acá, pero la versión de Java)
        /// Entrega un listado en formato de (Indice) (Ranks[]).
        /// Pero no agrega los ranks al indice.
        /// </summary>
        /// <param name="luceneIndexDirectory"></param>
        /// <param name="inputTriples"></param>
        /// <param name="ouputPath"></param>
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

        /// <summary>
        /// Handle toma cada linea del archivo NTriples.
        /// Por cada linea, pregunta si el diccionary ya tiene la llave (en rigor, debería tenerlas todas)
        /// Por cada entidad nueva, crea una lista llamada outLinkList.
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="diccionary"></param>
        /// <param name="graph"></param>
        public static void handleStatement(Triple s, Dictionary<string, int> diccionary, int[][] graph)
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
                if (diccionary.ContainsKey(name))
                {
                    currentIndex = diccionary.GetValueOrDefault(name);
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
                //Cuando hay un cambio en la entidad (last != subject)
                //Si outLinkList not null, not empty
                //Usando indice, que sacó desde el valor del diccionario(Qnum, indice)
                //Asigna grafo[indice] = lista.toArray(). Es decir, le asigna todos los valores de los outLinkList.
                //
                if (outLinksList != null && !outLinksList.Any())
                {
                    graph[currentIndex] = outLinksList.ToArray();
                }

                last = subject;
                var name = last;
                name = name.Replace(entityIRI, "");
                if (diccionary.ContainsKey(name))
                {
                    currentIndex = diccionary.GetValueOrDefault(name);
                    outLinksList = new List<int>();
                }
                else
                {
                    outLinksList = null;
                }
            }

            //Ahora, como asigna el valor en outLinkList?
            //Si el object es literal, nada.
            //Si la lista es vacía o el diccionario no tiene la llave de la entidad que apunta la propiedad, nada.
            //Si el diccionario si tiene la entidad(Objeto), agrega a la lista el valor del indice.

            // PROPERTIES
            if (s.Object.IsLiteral()) return;

            var ntobject = s.Object.ToString();
            var value = ntobject.Replace(entityIRI, "");

            if (outLinksList == null || !diccionary.ContainsKey(value)) return;

            var valueId = diccionary.GetValueOrDefault(value);

            if (!outLinksList.Contains(valueId))
                outLinksList.Add(valueId);
        }

        //private static double[] rankGraph(int[][] graph, int iterations)
        //{

        //}

        private static double[] rankGraph(int[][] graph)
        {
            var iterations = 25;
            var pageRankAlpha = 0.85d;
            var nodeCount = graph.Length;

            var oldRanks = new double[nodeCount];

            var initialRank = 1d / nodeCount;

            for (var i = 0; i < nodeCount; i++)
            {
                oldRanks[i] = initialRank;
            }

            double[] ranks = null;
            for (var i = 0; i < iterations; i++)
            {
                var noLinkRank = 0d;
                ranks = new double[nodeCount];

                for (var j = 0; j < nodeCount; j++)
                {
                    if (graph[j] != null)
                    {
                        var out1 = graph[j];
                        var share = oldRanks[j] * pageRankAlpha / out1.Length;
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

                var shareNoLink = (noLinkRank * pageRankAlpha) / nodeCount;

                var shareMinusD = (1d - pageRankAlpha) / nodeCount;

                var weakRank = shareNoLink + shareMinusD;

                var sum = 0d;
                var e = 0d;

                for (var k = 0; k < nodeCount; k++)
                {
                    ranks[k] += weakRank;
                    sum += ranks[k];
                    e += Math.Abs(oldRanks[k] - ranks[k]);
                }

                Array.Copy(ranks, 0, oldRanks, 0, nodeCount);
            }

            return ranks;
        }



        // INIT INDEX READER
        //    IndexReader reader = DirectoryReader.Open(FSDirectory.Open(Paths.get(dataDirectory)));
        //int graphLength = reader.MaxDoc;
        //Map<String, Integer> diccionary = new HashMap<>();
        //System.err.println("Graph size: " + graphLength);

        //// CREATE MAP TO TRANSLATE SUBJECT TO ID
        //System.err.println("Creating diccionary...");
        //for (int i = 0; i < graphLength; i++)
        //{
        //    Document doc = reader.document(i);
        //    String key = doc.get(DataFields.SUBJECT.name());
        //    diccionary.put(key, i);
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
        //RankHandler handler = new RankHandler(graph, diccionary);
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