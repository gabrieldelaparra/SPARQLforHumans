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
                    node.Types = node.uris.ToList();
                    node.GivenTypes = node.uris.ToList();
                }
                else if (node.IsInstanceOfType)
                {
                    node.Types = node.GetInstanceOfValues(graph).ToList();
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
                    edge.DomainDerivedTypes = edge.DomainDerivedTypes.IntersectIfAny(new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, edge.DomainBaseTypes, 20).Query(100).Select(x => x.Id)).ToList();
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
                    edge.RangeDerivedTypes = edge.RangeDerivedTypes.IntersectIfAny(new BatchIdEntityInstanceQuery(graph.EntitiesIndexPath, edge.RangeBaseTypes, 20).Query(100).Select(x => x.Id)).ToList();
                }

                ////////if (edge.IsGivenType) return QueryType.GivenPredicateTypeNoQuery;

                ////////case QueryType.GivenPredicateTypeNoQuery:
                ////////    edge.Domain = sourceNode.IsGivenType
                ////////        ? sourceNode.Types
                ////////        : InMemoryQueryEngine
                ////////            .BatchPropertyIdDomainTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                ////////    edge.Range = targetNode.IsGivenType
                ////////        ? targetNode.Types
                ////////        : InMemoryQueryEngine
                ////////            .BatchPropertyIdRangeTypesQuery(edge.uris.Select(x => x.GetUriIdentifier())).ToList();
                ////////    break;

                //if (edge.IsGivenType && !edge.IsInstanceOf)
                //{
                //    edge.Domain = source.IsGivenType
                //        ? source.Types
                //        : InMemoryQueryEngine.BatchPropertyIdDomainTypesQuery(edge.uris).ToList();
                //    edge.Range = target.IsGivenType
                //        ? target.Types
                //        : InMemoryQueryEngine.BatchPropertyIdRangeTypesQuery(edge.uris).ToList();
                //}

                //else
                //{
                //    if (source.IsGivenType)
                //        edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                //    if (target.IsGivenType)
                //        edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();
                //    if (source.IsInstanceOfType)
                //        edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                //    if (target.IsInstanceOfType)
                //        edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();
                //}

                ////////if (source.IsGivenType && target.IsGivenType) return QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties;
                ////////if (source.IsGivenType) return QueryType.GivenSubjectTypeDirectQueryOutgoingProperties;
                ////////if (target.IsGivenType) return QueryType.GivenObjectTypeDirectQueryIncomingProperties;

                ////////case QueryType.GivenSubjectTypeDirectQueryOutgoingProperties:
                ////////    edge.Domain = edge.GetSourceNode(graph).Types;
                ////////    edge.Range = new List<string>();
                ////////    break;
                ////////case QueryType.GivenObjectTypeDirectQueryIncomingProperties:
                ////////    edge.Domain = new List<string>();
                ////////    edge.Range = edge.GetTargetNode(graph).Types;
                ////////    break;
                ////////case QueryType.GivenSubjectAndObjectTypeDirectQueryIntersectOutInProperties:
                ////////    edge.Domain = edge.GetSourceNode(graph).Types;
                ////////    edge.Range = edge.GetTargetNode(graph).Types;
                ////////    break;

                //if (source.IsGivenType)
                //    edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                //if (target.IsGivenType)
                //    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();

                ////////if (source.IsInstanceOfType && target.IsInstanceOfType) return QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
                ////////if (source.IsInstanceOfType) return QueryType.KnownSubjectTypeQueryDomainProperties;
                ////////if (target.IsInstanceOfType) return QueryType.KnownObjectTypeQueryRangeProperties;

                ////////case QueryType.KnownSubjectTypeQueryDomainProperties:
                ////////    edge.Domain = edge.GetSourceNode(graph).Types;
                ////////    edge.Range = new List<string>();
                ////////    break;
                ////////case QueryType.KnownObjectTypeQueryRangeProperties:
                ////////    edge.Domain = new List<string>();
                ////////    edge.Range = edge.GetTargetNode(graph).Types;
                ////////    break;
                ////////case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                ////////    edge.Domain = edge.GetSourceNode(graph).Types;
                ////////    edge.Range = edge.GetTargetNode(graph).Types;
                ////////    break;

                //if (source.IsInstanceOfType)
                //    edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                //if (target.IsInstanceOfType)
                //    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();

                ////////switch (edge.QueryType)
                ////////{
                ////////    case QueryType.QueryTopProperties:
                ////////        edge.Domain = new MultiLabelTypeQuery(graph.EntitiesIndexPath, "*").Query().Select(x => x.Id)
                ////////            .ToList();
                ////////        edge.Range = edge.Domain.ToList();
                ////////        break;
                ////////}
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
                node.InferredBasedTypes = outgoingBaseTypes.IntersectIfAny(incomingBaseTypes).ToList();




                var outgoingTypes = outgoingEdges.SelectMany(x => x.Domain).ToList();
                var incomingTypes = incomingEdges.SelectMany(x => x.Range).ToList();
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
                var source = edge.GetSourceNode(graph);
                var target = edge.GetTargetNode(graph);

                //if (source.IsInferredType)
                //    edge.DomainBaseTypes

                if (source.IsInferredType)
                    edge.Domain = edge.Domain.IntersectIfAny(source.Types).ToList();
                if (target.IsInferredType)
                    edge.Range = edge.Range.IntersectIfAny(target.Types).ToList();

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
