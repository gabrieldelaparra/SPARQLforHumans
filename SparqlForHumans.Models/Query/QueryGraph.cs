using System.Linq;
using System.Collections.Generic;
using SparqlForHumans.Utilities;
namespace SparqlForHumans.Models.Query
{
    //TODO: Implement IsEqualityComparer<RDFExplorerGraph>, para saber si el graph cambió, luego procesar la otra parte.
    public class RDFExplorerGraph
    {
        public Node[] nodes { get; set; } = new Node[0];

        public Edge[] edges { get; set; } = new Edge[0];

        public Selected selected { get; set; } = new Selected();

        public override string ToString()
        {
            return $"{string.Join<Node>(";", nodes)}";
        }
    }

    public enum GraphQueryType
    {
        Unkwown,
        ConstantTypeDoNotQuery,
        QueryTopProperties,
        QueryTopEntities,
        KnownSubjectTypeOnlyQueryDomainProperties,
        KnownObjectTypeOnlyQueryRangeProperties,
        KnownSubjectAndObjectTypesIntersectDomainRangeProperties,
        KnownNodeTypeQueryInstanceEntities,
        KnownDomainTypeNotUsed,
        KnownNodeAndDomainTypesNotUsed,
    }

    //TODO: Implement IsEqualityComparer<QueryGraph>, para saber si el graph cambió
    public class QueryGraph
    {
        public QueryGraph(RDFExplorerGraph rdfGraph)
        {
            this.Edges = rdfGraph.edges.Select(x => new QueryEdge(x)).ToList();
            this.Nodes = rdfGraph.nodes.Select(x => new QueryNode(x)).ToList();
            this.Selected = rdfGraph.selected;
            this.ExploreGraph();
            this.Nodes.ForEach(x => this.TraverseDepthFirstNode(x.id));
            this.Edges.ForEach(x => this.TraverseDepthFirstEdge(x.id));
        }

        public List<QueryNode> Nodes { get; set; }
        public List<QueryEdge> Edges { get; set; }
        public Selected Selected { get; set; }

    }

    public class QueryNode : Node
    {
        public bool Traversed { get; set; } = false;
        public QueryNode(Node node)
        {
            this.id = node.id;
            this.name = node.name;
            this.uris = node.uris;
        }
        public GraphQueryType QueryType {get;set; } = GraphQueryType.Unkwown;
        public List<string> Results { get; set; } = new List<string>();
        public List<string> Types { get; set; } = new List<string>();
        public bool IsKnownType { get; set; } = false;
        public bool IsConnectedToKnownType { get; set; } = false;
        public override string ToString()
        {
            return $"{id}:{name} {(Types.Any() ? string.Join(";", Types.Select(x=>x.GetUriIdentifier())) : string.Empty)}: [HasP31:{IsKnownType}][edgeToP31:{IsConnectedToKnownType}] : {string.Join(",", Results)}";
        }
    }

    public class QueryEdge : Edge
    {
        public bool Traversed { get; set; } = false;
        public QueryEdge(Edge edge)
        {
            this.id = edge.id;
            this.name = edge.name;
            this.uris = edge.uris;
            this.sourceId = edge.sourceId;
            this.targetId = edge.targetId;
        }
        public GraphQueryType QueryType {get;set; } = GraphQueryType.Unkwown;
        public List<string> Results { get; set; } = new List<string>();
        public bool IsInstanceOf { get; set; } = false;
        public override string ToString()
        {
            return $"{id}:{name} : ({sourceId})->({targetId}) : [IsP31: {IsInstanceOf}] : {string.Join(",", Results)}";
        }
    }

    public static class GraphExtensions
    {
        internal static bool HasInstanceOf(this string[] uris)
        {
            return uris.Any(u => u.EndsWith("P31"));
        }
        internal static IEnumerable<Edge> GetInstanceOfEdges(this QueryNode node, QueryGraph graph)
        {
            return node.GetOutgoingEdges(graph)?.Where(x => x.IsInstanceOf);
        }
        internal static IEnumerable<string> GetInstanceOfValues(this QueryNode node, QueryGraph graph)
        {
            return node.GetInstanceOfEdges(graph)?.SelectMany(x => x.uris);
        }
        public static QueryNode GetSourceNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.Find(x => x.id.Equals(edge.sourceId));
        }
        internal static QueryNode GetTargetNode(this QueryEdge edge, QueryGraph graph)
        {
            return graph.Nodes.Find(x => x.id.Equals(edge.targetId));
        }
        internal static IEnumerable<QueryEdge> GetOutgoingEdges(this QueryNode node, QueryGraph graph)
        {
            return graph.Edges.Where(x => x.sourceId.Equals(node.id));
        }

        internal static IEnumerable<QueryNode> GetConnectedNodes(this QueryNode node, QueryGraph graph)
        {
            var edges = node.GetOutgoingEdges(graph);
            if (edges == null) return null;
            return edges.Select(x => x.GetTargetNode(graph));
        }

        internal static void FillNodeResults(this QueryNode node, QueryGraph graph)
        {
            // IsKnownType? -> Flag -> DomainTypes 
            // ConnectsToKnownTypes? -> Flag -> RangeTypes (Este caso no se como funciona?) 
            // Check Case (OnlyDomain, OnlyRange, Both)
            // Fill results
            if(node.uris.Any())
                node.QueryType = GraphQueryType.ConstantTypeDoNotQuery;
            else if (node.IsKnownType && node.IsConnectedToKnownType)
                node.QueryType = GraphQueryType.KnownNodeAndDomainTypesNotUsed;
            else if (node.IsKnownType)
                node.QueryType = GraphQueryType.KnownNodeTypeQueryInstanceEntities;
            else if (node.IsConnectedToKnownType)
                node.QueryType = GraphQueryType.KnownDomainTypeNotUsed;
            else
                node.QueryType = GraphQueryType.QueryTopEntities;
        }

        internal static void FillEdgeResults(this QueryEdge edge, QueryGraph graph)
        {
            // IsKnownType? -> Flag -> DomainTypes 
            // ConnectsToKnownTypes? -> Flag -> RangeTypes (Este caso no se como funciona?) 
            // Check Case (OnlyDomain, OnlyRange, Both)
            // Fill results
            var source = edge.GetSourceNode(graph);
            var target = edge.GetTargetNode(graph);

            if(edge.uris.Any())
                edge.QueryType = GraphQueryType.ConstantTypeDoNotQuery;
            else if (source.IsKnownType && target.IsKnownType)
                edge.QueryType = GraphQueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            else if (source.IsKnownType)
                edge.QueryType = GraphQueryType.KnownSubjectTypeOnlyQueryDomainProperties;
            else if (target.IsKnownType)
                edge.QueryType = GraphQueryType.KnownObjectTypeOnlyQueryRangeProperties;
            else
                edge.QueryType = GraphQueryType.QueryTopProperties;
        }

        internal static void ExploreGraph(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges)
            {
                if (edge.uris.HasInstanceOf())
                    edge.IsInstanceOf = true;
            }
            foreach (var node in graph.Nodes)
            {
                if (node.GetInstanceOfValues(graph).Any())
                {
                    node.IsKnownType = true;
                    node.Types = node.GetConnectedNodes(graph).SelectMany(x=>x.uris).Distinct().ToList();
                }
            }

            foreach (var node in graph.Nodes)
            {
                if (node.GetConnectedNodes(graph).Any(x => x.IsKnownType))
                    node.IsConnectedToKnownType = true;
            }

        }

        internal static void TraverseDepthFirstNode(this QueryGraph graph, int nodeId)
        {
            // Get the Node
            var node = graph.Nodes.Find(x => x.id.Equals(nodeId));
            if (node.Traversed) return;

            // Check rules for this Node:
            node.FillNodeResults(graph);

            // Mark as Checked
            node.Traversed = true;

            // Recursion
            foreach (var edge in node.GetOutgoingEdges(graph).Where(x => !x.Traversed))
            {
                TraverseDepthFirstEdge(graph, edge.id);
            }
        }

        internal static void TraverseDepthFirstEdge(this QueryGraph graph, int edgeId)
        {
            // Get the Edge
            var edge = graph.Edges.Find(x => x.id.Equals(edgeId));
            if (edge.Traversed) return;

            // Check rules for this Edge:
            edge.FillEdgeResults(graph);

            // Mark as Checked
            edge.Traversed = true;

            // Recursion
            var node = edge.GetTargetNode(graph);
            if (!node.Traversed)
                TraverseDepthFirstNode(graph, node.id);
        }
    }


}
