using SparqlForHumans.Models;
using SparqlForHumans.Models.Query;
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
        public static void GetGraphQueryResults(this QueryGraph graph, string entitiesIndexPath, string propertiesIndexPath)
        {
            graph.RunNodeQueries(entitiesIndexPath);
            graph.RunEdgeQueries(propertiesIndexPath);
        }
        public static void RunNodeQueries(this QueryGraph graph, string indexPath)
        {
            foreach (var node in graph.Nodes)
            {
                switch (node.QueryType)
                {
                    case QueryType.KnownNodeTypeQueryInstanceEntities:
                        node.Results = new MultiLabelTypeQuery(indexPath, node.Types.FirstOrDefault().GetUriIdentifier()).Query();
                        break;
                    case QueryType.QueryTopEntities:
                        node.Results = new MultiLabelEntityQuery(indexPath, "*").Query();
                        break;
                    case QueryType.KnownSubjectTypeOnlyQueryDomainProperties:
                    case QueryType.KnownObjectTypeOnlyQueryRangeProperties:
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                    case QueryType.Unkwown:
                    case QueryType.QueryTopProperties:
                    case QueryType.ConstantTypeDoNotQuery:
                    case QueryType.KnownDomainTypeNotUsed:
                    case QueryType.KnownNodeAndDomainTypesNotUsed:
                    default:
                        break;
                }
            }
        }

        public static void RunEdgeQueries(this QueryGraph graph, string indexPath)
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
                        domainProperties = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        edge.Results = domainProperties;
                        break;
                    case QueryType.KnownObjectTypeOnlyQueryRangeProperties:
                        targetUri = edge.GetTargetNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        rangeProperties = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        edge.Results = rangeProperties.ToList();
                        break;
                    case QueryType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties:
                        sourceUri = edge.GetSourceNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        domainProperties = new MultiDomainPropertyQuery(indexPath, sourceUri).Query();
                        targetUri = edge.GetTargetNode(graph).Types.FirstOrDefault().GetUriIdentifier();
                        rangeProperties = new MultiRangePropertyQuery(indexPath, targetUri).Query();
                        var properties = rangeProperties.Intersect(domainProperties, new PropertyComparer());
                        edge.Results = properties.ToList();
                        break;
                    case QueryType.Unkwown:
                    case QueryType.KnownNodeTypeQueryInstanceEntities:
                    case QueryType.QueryTopEntities:
                    case QueryType.ConstantTypeDoNotQuery:
                    case QueryType.KnownDomainTypeNotUsed:
                    case QueryType.KnownNodeAndDomainTypesNotUsed:
                    default:
                        break;
                }
            }
        }
    }
}
