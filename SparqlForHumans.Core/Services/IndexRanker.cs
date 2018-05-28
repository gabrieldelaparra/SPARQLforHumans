using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace SparqlForHumans.Core.Services
{
   public  class IndexRanker
    {
        public static void Rank(Lucene.Net.Store.Directory luceneIndexDirectory, string inputTriples, string ouputPath)
        {
            using (var indexReader = IndexReader.Open(luceneIndexDirectory, readOnly: false))
            {

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
}
