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
                {
                    node.GivenTypes = node.uris.ToList();
                    node.InstanceOfBaseTypes = new BatchIdEntityQuery(graph.EntitiesIndexPath, node.GivenTypes).Query().SelectMany(x => x.InstanceOf).ToList();
                }
                if (node.IsInstanceOfType)
                {
                    node.InstanceOfBaseTypes = node.InstanceOfBaseTypes.IntersectIfAny(node.GetInstanceOfValues(graph).ToList()).ToList();
                }
            }
        }

        internal static void SetBaseEdgeDomainRanges(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                //If the source is given, limit the domain and range to the types of that entity.
                if (source.IsGivenType)
                {
                    edge.DomainBaseTypes = source.InstanceOfBaseTypes;
                    edge.DomainDerivedTypes = source.GivenTypes;
                }
                else // !source.IsGivenType
                {
                    if (edge.IsGivenType)
                    {
                        //TODO: if edge.InstanceOf or Other
                        edge.DomainBaseTypes = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris).ToList();
                    }
                    else // !source.IsGivenType && !edge.IsGivenType
                    {
                        if (source.IsInstanceOfType)
                        {
                            //TODO: This should be somewhere else:
                            edge.DomainBaseTypes = source.InstanceOfBaseTypes;
                        }
                        else // !source.IsGivenType && !edge.IsGivenType && !source.IsInstanceOfType
                        {
                            //No DomainTypes. We know nothing.
                        }
                    }
                }

                //If the target is given, limit the domain and range to the types of that entity.
                if (target.IsGivenType)
                {
                    if (edge.IsInstanceOf)
                    {
                        edge.RangeBaseTypes = target.GivenTypes;
                    }
                    else //!edge.IsInstanceOf
                    {
                        edge.RangeBaseTypes = target.InstanceOfBaseTypes;
                        edge.RangeDerivedTypes = target.GivenTypes;
                    }
                }
                else // !target.IsGivenType
                {
                    if (edge.IsGivenType)
                    {
                        edge.RangeBaseTypes = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris).ToList();
                    }
                    else  // !target.IsGivenType && !edge.IsGivenType
                    {
                        if (target.IsInstanceOfType)
                        {
                            edge.RangeBaseTypes = target.InstanceOfBaseTypes;
                        }
                        else// !target.IsGivenType && !edge.IsGivenType && !target.IsInstanceOfTypes
                        {
                            //No RangeTypes. We know nothing.
                        }
                    }

                }
            }
        }

        internal static void SetInferredNodeTypes(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();
                var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();

                //TODO: Not sure if I should do this
                foreach (var incomingEdge in incomingEdges)
                {
                    node.InferredBaseTypes = node.InferredBaseTypes.IntersectIfAny(incomingEdge.RangeBaseTypes).ToList();
                }
                foreach (var outgoingEdge in outgoingEdges)
                {
                    node.InferredBaseTypes = node.InferredBaseTypes.IntersectIfAny(outgoingEdge.DomainBaseTypes).ToList();
                }
            }
        }

        internal static void SetInferredEdgeTypes(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                if (source.IsInferredType)
                    edge.DomainBaseTypes = edge.DomainBaseTypes.IntersectIfAny(source.InferredBaseTypes).ToList();
                if (target.IsInferredType)
                    edge.RangeBaseTypes = edge.RangeBaseTypes.IntersectIfAny(target.InferredBaseTypes).ToList();
            }
        }

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
    }
}
