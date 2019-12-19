using System;
using System.Collections.Generic;
using System.Linq;
using SparqlForHumans.Models.RDFExplorer;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;

namespace SparqlForHumans.Benchmark
{
    public class SparqlToGraphConverter
    {
        public RDFExplorerGraph ConvertToGraph(string query)
        {
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.Namespaces.AddNamespace("wd", new Uri("http://www.wikidata.org/entity/"));
            queryString.Namespaces.AddNamespace("wdt", new Uri("http://www.wikidata.org/prop/direct/"));
            queryString.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            queryString.CommandText = query;

            var parser = new SparqlQueryParser();

            //Then we can parse a SPARQL string into a query
            var sparqlQuery = parser.ParseFromString(queryString);

            var triples = sparqlQuery.RootGraphPattern.TriplePatterns.Select(x => (TriplePattern)x).ToArray();
            var variables = sparqlQuery.RootGraphPattern.Variables.ToArray();

            var nodesDict = new Dictionary<string, Node>();
            var edgesDict = new Dictionary<string, Edge>();

            foreach (var triple in triples)
            {
                var s = triple.Subject.ToString();
                var p = triple.Predicate.ToString();
                var o = triple.Object.ToString();

                if (!nodesDict.ContainsKey(s))
                    nodesDict.Add(s, ToNode(s));

                if (!nodesDict.ContainsKey(o))
                    nodesDict.Add(o, ToNode(o));

                var subjectNode = nodesDict[s];
                var objectNode = nodesDict[o];

                if (!edgesDict.ContainsKey(p))
                    edgesDict.Add(p, ToEdge(p, subjectNode.id, objectNode.id));
            }

            var nodes = nodesDict.Select(x => x.Value).ToArray();
            var edges = edgesDict.Select(x => x.Value).ToArray();

            return new RDFExplorerGraph { nodes = nodes.ToArray(), edges = edges.ToArray() };
        }

        private int _nodeIndex = 1;
        private int _edgeIndex = 1;

        private Node ToNode(string item)
        {
            var variableName = "var" + _nodeIndex;
            var node = new Node(_nodeIndex++, variableName);
            if (item.StartsWith("<http"))
                node.uris = new[] { item.TrimStart('<').TrimEnd('>') };
            return node;
        }

        private Edge ToEdge(string item, int from, int to)
        {
            var propName = "prop" + _edgeIndex;
            var edge = new Edge(_edgeIndex++, propName, @from, to);
            if (item.StartsWith("<http"))
                edge.uris = new[] { item.TrimStart('<').TrimEnd('>') };
            return edge;
        }
    }
}
