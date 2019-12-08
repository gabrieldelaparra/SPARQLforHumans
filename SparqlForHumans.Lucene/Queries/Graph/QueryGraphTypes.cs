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
                    //node.Types = node.uris.ToList();
                    node.GivenTypes = node.uris.ToList();
                }
                else if (node.IsInstanceOfType)
                {
                    //node.Types = node.GetInstanceOfValues(graph).ToList();
                    node.InstanceOfBaseTypes = node.GetInstanceOfValues(graph).ToList();
                    //TODO: Get the derived types here?
                }
            }
        }

        internal static void SetBaseEdgeDomainRanges(this QueryGraph graph)
        {
            foreach (var edge in graph.Edges.Select(x => x.Value))
            {
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                //Get all the Domain and Ranges of the given property.
                if (edge.IsGivenType)
                {
                    edge.DomainBaseTypes = InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris).ToList();
                    edge.RangeBaseTypes = InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris).ToList();
                }

                //Now, if the source is instanceOf (from another edge), intersect those domain types:
                if (source.IsInstanceOfType && !source.IsGivenType && !edge.IsInstanceOf)
                {
                    edge.DomainBaseTypes = edge.DomainBaseTypes.IntersectIfAny(source.InstanceOfBaseTypes).ToList();
                }
                //If the target is also InstanceOf (to another node), intersect those range types:
                if (target.IsInstanceOfType && !target.IsGivenType)
                {
                    edge.RangeBaseTypes = edge.RangeBaseTypes.IntersectIfAny(target.InstanceOfBaseTypes).ToList();
                }

                if (source.IsGivenType)
                {
                    //TODO: Not sure if should intersect:
                    //edge.DomainDerivedTypes = edge.DomainDerivedTypes.IntersectIfAny(source.GivenTypes).ToList();
                    edge.DomainDerivedTypes = source.GivenTypes;
                }
                else
                {
                    //TODO: Maybe they should be retrieved in the end, before getting the results (and after filtering if required)
                    //edge.DomainDerivedTypes = edge.DomainDerivedTypes.IntersectIfAny(new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, edge.DomainBaseTypes, 20).Query(100).Select(x => x.Id)).ToList();
                }

                if (target.IsGivenType && !edge.IsInstanceOf)
                {
                    //TODO: Not sure if should intersect:
                    //edge.RangeDerivedTypes = edge.DomainDerivedTypes.IntersectIfAny(target.GivenTypes).ToList();
                    edge.RangeDerivedTypes = target.GivenTypes;
                }
                else
                {
                    //TODO: Maybe they should be retrieved in the end, before getting the results (and after filtering if required)
                    //edge.RangeDerivedTypes = edge.RangeDerivedTypes.IntersectIfAny(new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, edge.RangeBaseTypes, 20).Query(100).Select(x => x.Id)).ToList();
                }
            }
        }

        internal static void SetInferredNodeTypes(this QueryGraph graph)
        {
            foreach (var node in graph.Nodes.Select(x => x.Value))
            {
                var outgoingEdges = node.GetOutgoingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();
                var incomingEdges = node.GetIncomingEdges(graph).Where(x => !x.IsInstanceOf).ToArray();

                //Inferred Domain Types
                var outgoingBaseTypes = outgoingEdges.SelectMany(x => x.DomainBaseTypes).ToList();
                //Inferred Range Types
                var incomingBaseTypes = incomingEdges.SelectMany(x => x.RangeBaseTypes).ToList();
                //Intersect
                node.InferredBaseTypes = outgoingBaseTypes.IntersectIfAny(incomingBaseTypes).ToList();
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

                //if (source.IsInferredType)
                //    edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                //if (target.IsInferredType)
                //    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();
            }
        }



        public static void SetTypesDomainsAndRanges(this QueryGraph graph)
        {
            InMemoryQueryEngine.Init(graph.EntitiesIndexPath, graph.PropertiesIndexPath);
            graph.SetBaseNodeTypes();
            graph.SetBaseEdgeDomainRanges();
            graph.SetInferredNodeTypes();
            graph.SetInferredEdgeTypes();
        }
    }
}
