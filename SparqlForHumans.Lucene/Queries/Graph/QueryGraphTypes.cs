using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphTypes
    {
        internal static void SetBaseNodeTypes(this QueryGraph graph)
        {
            //If IsGivenType, get those types
            //If IsInstanceOfType (P31 to Type), Get those Types
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                switch (node.QueryType)
                {
                    case QueryType.GivenEntityTypeNoQuery:
                        node.Types = node.uris.ToList();
                        break;
                    case QueryType.QueryTopEntities:
                        node.Types = new List<string>();
                        break;
                    case QueryType.GivenSubjectTypeQueryDirectlyEntities:
                        node.Types = new List<string>();
                        break;
                    case QueryType.GivenObjectTypeQueryDirectlyEntities:
                        node.Types = new List<string>();
                        break;
                    case QueryType.SubjectIsInstanceOfTypeQueryEntities:
                        node.Types = node.GetInstanceOfValues(graph).ToList();
                        break;
                }
            }
        }

        internal static void SetInferredNodeTypes(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                switch (node.QueryType)
                {
                    case QueryType.InferredDomainTypeEntities:
                        node.Types = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                        node.Types = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain)
                            .Intersect(node.GetIncomingEdges(graph).SelectMany(x => x.Range)).ToList();
                        break;
                    case QueryType.InferredRangeTypeEntities:
                        node.Types = node.GetIncomingEdges(graph).SelectMany(x => x.Range).ToList();
                        break;
                    case QueryType.DirectQuery:
                        node.Types = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain)
                            .Intersect(node.GetIncomingEdges(graph).SelectMany(x => x.Range)).ToList();
                        break;
                }
            }
        }

        internal static void SetInferredEdgeTypes(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                QueryNode sourceNode;
                QueryNode targetNode;
                switch (edge.QueryType)
                {
                    case QueryType.InferredDomainAndRangeTypeProperties:
                        sourceNode = edge.GetSourceNode(graph);
                        targetNode = edge.GetTargetNode(graph);
                        edge.Domain = sourceNode.Types;
                        edge.Range = targetNode.Types;
                        break;
                    case QueryType.InferredDomainTypeProperties:
                        sourceNode = edge.GetSourceNode(graph);
                        edge.Domain = sourceNode.Types;
                        edge.Range = new List<string>();
                        break;
                    case QueryType.InferredRangeTypeProperties:
                        targetNode = edge.GetTargetNode(graph);
                        edge.Domain = new List<string>();
                        edge.Range = targetNode.Types;
                        break;
                }
            }
        }

        internal static void SetEdgeDomainRanges(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                switch (edge.QueryType)
                {
                    case QueryType.GivenPredicateTypeNoQuery:
                        var sourceNode = edge.GetSourceNode(graph);
                        var targetNode = edge.GetTargetNode(graph);
                        edge.Domain = sourceNode.IsGivenType
                            ? sourceNode.Types
                            : InMemoryQueryEngine
                                .BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                        edge.Range = targetNode.IsGivenType
                            ? targetNode.Types
                            : InMemoryQueryEngine
                                .BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                        break;
                    case QueryType.QueryTopProperties:
                        edge.Domain = new MultiLabelTypeQuery(graph.EntitiesIndexPath, "*").Query().Select(x => x.Id)
                            .ToList();
                        edge.Range = edge.Domain.ToList();
                        break;
                    case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                        edge.Domain = edge.GetSourceNode(graph).Types;
                        edge.Range = new List<string>();
                        break;
                    case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                        edge.Domain = new List<string>();
                        edge.Range = edge.GetTargetNode(graph).Types;
                        break;
                    case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                        edge.Domain = edge.GetSourceNode(graph).Types;
                        edge.Range = edge.GetTargetNode(graph).Types;
                        break;
                    case QueryType.KnownSubjectTypeQueryDomainProperties:
                        edge.Domain = edge.GetSourceNode(graph).Types;
                        edge.Range = new List<string>();
                        break;
                    case QueryType.KnownObjectTypeQueryRangeProperties:
                        edge.Domain = new List<string>();
                        edge.Range = edge.GetTargetNode(graph).Types;
                        break;
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                        edge.Domain = edge.GetSourceNode(graph).Types;
                        edge.Range = edge.GetTargetNode(graph).Types;
                        break;
                }
            }
        }

        public static void SetTypesDomainsAndRanges(this QueryGraph graph)
        {
            InMemoryQueryEngine.Init(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseNodeTypes();
            graph.SetEdgeDomainRanges();
            graph.SetInferredNodeTypes();
            graph.SetInferredEdgeTypes();
        }
    }
}
