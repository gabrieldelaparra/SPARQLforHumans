using SparqlForHumans.Models;
using SparqlForHumans.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SparqlForHumans.Lucene.Queries.Graph
{

    /// Case 1:
    /// ?var0
    /// ?var1
    /// ?var0 -> ?prop -> ?var1
    /// 
    /// Case 2: P31 and prop going from the same node
    /// ?var0 -> P31 -> Qxx
    /// ?var0 -> ?prop -> ?var1
    ///
    /// Case 3: P31 going from a different node.
    /// ?var1 -> ?prop -> ?var0
    ///                   ?var0 -> P31 -> Qxx
    /// Case 4: P31 going ot from both nodes
    /// ?var0 -> P31 -> Qxx
    ///                   ?var1 -> P31 -> Qyy
    /// ?var0 -> ?prop -> ?var1
    public static class GraphQueryResults
    {
        public static void RunGraphQueryResults(this QueryGraph graph)
        {
            graph.RunNodeQueries(graph.EntitiesIndexPath);
            graph.RunEdgeQueries(graph.PropertiesIndexPath);
        }
        private static void RunNodeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var node in graph.Nodes)
            {
                switch (node.QueryType)
                {
                    case QueryType.KnownSubjectTypeQueryInstanceEntities:
                    case QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new MultiIdInstanceOfEntityQuery(indexPath, node.Types.FirstOrDefault().GetUriIdentifier()).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new MultiLabelEntityQuery(indexPath, "*").Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                        //This should be done with the Wikipedia Endpoint
                        node.Results = new MultiIdInstanceOfEntityQuery(indexPath, node.InferredTypes.FirstOrDefault().GetUriIdentifier()).Query();
                        break;
                    case QueryType.InferredDomainAndRangeTypeProperties:
                    case QueryType.InferredDomainTypeProperties:
                    case QueryType.InferredRangeTypeProperties:
                    case QueryType.KnownSubjectTypeOnlyQueryDomainProperties:
                    case QueryType.KnownObjectTypeOnlyQueryRangeProperties:
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                    case QueryType.Unkwown:
                    case QueryType.QueryTopProperties:
                    case QueryType.ConstantTypeDoNotQuery:
                    case QueryType.KnownObjectTypeNotUsed:
                    default:
                        break;
                }
            }
        }

        private static void RunEdgeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var edge in graph.Edges)
            {
                string targetUri = string.Empty;
                string sourceUri = string.Empty;
                List<Property> domainProperties = new List<Property>(0);
                List<Property> rangeProperties = new List<Property>(0);
                switch (edge.QueryType)
                {
                    case QueryType.QueryTopProperties:
                        edge.Results = new MultiLabelPropertyQuery(indexPath, "*").Query();
                        break;
                    case QueryType.KnownSubjectTypeOnlyQueryDomainProperties:
                        sourceUri = edge.GetSourceNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        edge.Results = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        break;
                    case QueryType.KnownObjectTypeOnlyQueryRangeProperties:
                        targetUri = edge.GetTargetNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        edge.Results = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        break;
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                        sourceUri = edge.GetSourceNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        domainProperties = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        targetUri = edge.GetTargetNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        rangeProperties = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.InferredDomainAndRangeTypeProperties:
                        //TODO: Implement BatchDomain and BatchRange
                        sourceUri = edge.GetSourceNode(graph).InferredTypes.FirstOrDefault().GetUriIdentifier();
                        domainProperties = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        targetUri = edge.GetTargetNode(graph).InferredTypes.FirstOrDefault().GetUriIdentifier();
                        rangeProperties = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        edge.Results = rangeProperties.Intersect(domainProperties, new PropertyComparer()).ToList();
                        break;
                    case QueryType.InferredDomainTypeProperties:
                        sourceUri = edge.GetSourceNode(graph).InferredTypes.FirstOrDefault().GetUriIdentifier();
                        edge.Results = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        break;
                    case QueryType.InferredRangeTypeProperties:
                        targetUri = edge.GetTargetNode(graph).InferredTypes.FirstOrDefault().GetUriIdentifier();
                        edge.Results = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        break;
                    case QueryType.InferredDomainTypeEntities:
                    case QueryType.InferredDomainAndRangeTypeEntities:
                    case QueryType.InferredRangeTypeEntities:
                    case QueryType.Unkwown:
                    case QueryType.KnownSubjectTypeQueryInstanceEntities:
                    case QueryType.QueryTopEntities:
                    case QueryType.ConstantTypeDoNotQuery:
                    case QueryType.KnownObjectTypeNotUsed:
                    case QueryType.KnownSubjectAndObjectTypesQueryInstanceEntities:
                    default:
                        break;
                }
            }
        }
    }
}
