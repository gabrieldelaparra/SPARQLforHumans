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
                if (node.IsGivenType)
                    node.Types = node.uris.ToList();
                else if (node.IsInstanceOfType)
                    node.Types = node.GetInstanceOfValues(graph).ToList();
            }
        }

        internal static void SetInferredNodeTypes(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                //Inferred Domain Types
                var outgoingTypes = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                //Inferred Range Types
                var incomingTypes = node.GetIncomingEdges(graph).SelectMany(x => x.Range).ToList();
                //Intersect
                node.Types = node.Types.IntersectIfAny(outgoingTypes).IntersectIfAny(incomingTypes).ToList();

                //switch (node.QueryType) {

                //    case QueryType.InferredDomainTypeEntities:
                //        node.Types = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                //        break;
                //    case QueryType.InferredDomainAndRangeTypeEntities:
                //        node.Types = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain)
                //            .IntersectIfAny(node.GetIncomingEdges(graph).SelectMany(x => x.Range)).ToList();
                //        break;
                //    case QueryType.InferredRangeTypeEntities:
                //        node.Types = node.GetIncomingEdges(graph).SelectMany(x => x.Range).ToList();
                //        break;
                //    case QueryType.DirectQuery:
                //         outgoingTypes = node.GetOutgoingEdges(graph).SelectMany(x => x.Domain).ToList();
                //        node.Types = outgoingTypes.IntersectIfAny(incomingTypes).ToList();
                //        break;
                //}
            }
        }

        internal static void SetInferredEdgeTypes(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {

                var sourceNode = edge.GetSourceNode(graph);
                var targetNode = edge.GetTargetNode(graph);

                if(sourceNode.IsInferredType)
                    edge.Domain = edge.Domain.IntersectIfAny(sourceNode.Types).ToList();
                if (targetNode.IsInferredType)
                    edge.Range = edge.Range.IntersectIfAny(targetNode.Types).ToList();

                //if (source.IsInferredType && target.IsInferredType) return QueryType.InferredDomainAndRangeTypeProperties;
                //if (source.IsInferredType) return QueryType.InferredDomainTypeProperties;
                //if (target.IsInferredType) return QueryType.InferredRangeTypeProperties;

                //switch (edge.QueryType)
                //{
                //    case QueryType.InferredDomainAndRangeTypeProperties:
                //        sourceNode = edge.GetSourceNode(graph);
                //        targetNode = edge.GetTargetNode(graph);
                //        edge.Domain = sourceNode.Types;
                //        edge.Range = targetNode.Types;
                //        break;
                //    case QueryType.InferredDomainTypeProperties:
                //        sourceNode = edge.GetSourceNode(graph);
                //        edge.Domain = sourceNode.Types;
                //        edge.Range = new List<string>();
                //        break;
                //    case QueryType.InferredRangeTypeProperties:
                //        targetNode = edge.GetTargetNode(graph);
                //        edge.Domain = new List<string>();
                //        edge.Range = targetNode.Types;
                //        break;
                //}
            }
        }

        internal static void SetEdgeDomainRanges(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                //if (edge.IsGivenType) return QueryType.GivenPredicateTypeNoQuery;

                //case QueryType.GivenPredicateTypeNoQuery:
                //    edge.Domain = sourceNode.IsGivenType
                //        ? sourceNode.Types
                //        : InMemoryQueryEngine
                //            .BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                //    edge.Range = targetNode.IsGivenType
                //        ? targetNode.Types
                //        : InMemoryQueryEngine
                //            .BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                //    break;

                if (edge.IsGivenType) {
                    edge.Domain = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris).ToList();
                    edge.Range = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris).ToList();
                }

                //if (source.IsGivenType && target.IsGivenType) return QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties;
                //if (source.IsGivenType) return QueryType.GivenSubjectTypeDirectQueryOutgoingProperties;
                //if (target.IsGivenType) return QueryType.GivenObjectTypeDirectQueryIncomingProperties;

                //case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                //    edge.Domain = edge.GetSourceNode(graph).Types;
                //    edge.Range = new List<string>();
                //    break;
                //case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                //    edge.Domain = new List<string>();
                //    edge.Range = edge.GetTargetNode(graph).Types;
                //    break;
                //case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                //    edge.Domain = edge.GetSourceNode(graph).Types;
                //    edge.Range = edge.GetTargetNode(graph).Types;
                //    break;

                if(source.IsGivenType)
                    edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                if(target.IsGivenType)
                    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();

                //if (source.IsInstanceOfType && target.IsInstanceOfType) return QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
                //if (source.IsInstanceOfType) return QueryType.KnownSubjectTypeQueryDomainProperties;
                //if (target.IsInstanceOfType) return QueryType.KnownObjectTypeQueryRangeProperties;

                //case QueryType.KnownSubjectTypeQueryDomainProperties:
                //    edge.Domain = edge.GetSourceNode(graph).Types;
                //    edge.Range = new List<string>();
                //    break;
                //case QueryType.KnownObjectTypeQueryRangeProperties:
                //    edge.Domain = new List<string>();
                //    edge.Range = edge.GetTargetNode(graph).Types;
                //    break;
                //case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                //    edge.Domain = edge.GetSourceNode(graph).Types;
                //    edge.Range = edge.GetTargetNode(graph).Types;
                //    break;

                if (source.IsInstanceOfType)
                    edge.Domain =  edge.Domain.IntersectIfAny(source.Types).ToList();
                if (target.IsInstanceOfType)
                    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();

                //switch (edge.QueryType)
                //{
                //    case QueryType.QueryTopProperties:
                //        edge.Domain = new MultiLabelTypeQuery(graph.EntitiesIndexPath, "*").Query().Select(x => x.Id)
                //            .ToList();
                //        edge.Range = edge.Domain.ToList();
                //        break;
                //}
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
