using System.Linq;
using SparqlForHumans.Lucene.Extensions;
using SparqlForHumans.Lucene.Models;
using SparqlForHumans.Utilities;

namespace SparqlForHumans.Lucene.Queries.Graph
{
    public static class QueryGraphTypes
    {
        public static void SetTypesDomainsAndRanges(this QueryGraph graph)
        {
            InMemoryQueryEngine.Init(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseNodeTypes();
            graph.SetBaseEdgeDomainRanges();

            graph.SetInferredNodeTypes();
            graph.SetInferredEdgeTypes();
            graph.SetInferredNodeTypes();
            graph.SetInferredEdgeTypes();
        }

        internal static void SetBaseEdgeDomainRanges(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value)) {
                var sourceNode = edge.GetSourceNode(graph);
                var targetNode = edge.GetTargetNode(graph);

                //If the source is given, limit the domain and range to the types of that entity.
                if (sourceNode.IsConstant)
                    edge.DomainTypes = sourceNode.ParentTypes;
                //edge.DomainDerivedTypes = sourceNode.GivenTypes;
                // !source.IsGivenType
                else if (edge.IsConstant)
                    //TODO: if edge.InstanceOf or Other
                    edge.DomainTypes = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris).ToList();
                // !source.IsGivenType && !edge.IsGivenType
                else if (sourceNode.IsInstanceOf)
                    //TODO: This should be somewhere else:
                    edge.DomainTypes = sourceNode.ParentTypes;
                // !source.IsGivenType && !edge.IsGivenType && !source.IsInstanceOfType


                //If the target is given, limit the domain and range to the types of that entity.
                if (targetNode.IsConstant) {
                    if (edge.IsInstanceOf)
                        edge.RangeTypes = targetNode.Types;
                    //!edge.IsInstanceOf
                    else
                        edge.RangeTypes = targetNode.ParentTypes;
                }
                // !target.IsGivenType
                else if (edge.IsConstant) {
                    edge.RangeTypes = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris).ToList();
                }
                // !target.IsGivenType && !edge.IsGivenType
                else if (targetNode.IsInstanceOf) {
                    edge.RangeTypes = targetNode.ParentTypes;
                }
                // !target.IsGivenType && !edge.IsGivenType && !target.IsInstanceOfTypes
            }
        }

        internal static void SetBaseNodeTypes(this QueryGraph graph)
        {
            //If IsGivenType, get those types
            //If IsInstanceOfType (P31 to Type), Get those Types
            foreach (var node in graph.Nodes.Select(x => x.Value)) {
                if (node.IsConstant) {
                    node.Types = node.uris.ToList();
                    node.ParentTypes = new BatchIdEntityQuery(graph.EntitiesIndexPath, node.Types).Query()
                        .SelectMany(x => x.InstanceOf).ToList();
                }

                //TODO: Can I have this two conditions? IsInstanceOf and GivenType? Why not if..else ?
                if (node.IsInstanceOf)
                    node.ParentTypes = node.ParentTypes.IntersectIfAny(node.GetInstanceOfValues(graph)).ToList();
            }
        }

        internal static void SetInferredEdgeTypes(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value)) {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                if (source.IsInferredType)
                    edge.DomainTypes = edge.DomainTypes.IntersectIfAny(source.InferredTypes).ToList();
                if (target.IsInferredType)
                    edge.RangeTypes = edge.RangeTypes.IntersectIfAny(target.InferredTypes).ToList();
            }
        }

        internal static void SetInferredNodeTypes(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value)) {
                var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();
                var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();

                //TODO: Not sure if I should do this
                foreach (var incomingEdge in incomingEdges)
                    node.InferredTypes = node.InferredTypes.IntersectIfAny(incomingEdge.RangeTypes).ToList();
                foreach (var outgoingEdge in outgoingEdges)
                    node.InferredTypes = node.InferredTypes.IntersectIfAny(outgoingEdge.DomainTypes).ToList();
            }
        }
    }
}