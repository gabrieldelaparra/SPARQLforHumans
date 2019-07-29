using SparqlForHumans.Models.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public static class QueryGraphParser
    {
        public static GraphQueryType GetQueryType(RDFExplorerGraph graph)
        {
            //No edges, no nodes = Invalid graph
            //if(!graph.nodes.Any() && !graph.edges.Any()) 
            //    return GraphQueryType.Invalid;

            ////At least, one node should be there. There can not be edges without nodes.
            //if(!graph.edges.Any()) 
            //    return QueryGraphType.QueryTopEntities;
            
            ////Edges that have uri: P31
            //var instanceOfEdges = graph.edges.Where(e => e.uris.Any(u => u.EndsWith("P31")));

            //// No instanceOf
            //if (!instanceOfEdges.Any())
            //{
            //    return graph.selected.isNode ? QueryGraphType.QueryTopEntities :QueryGraphType.QueryTopProperties;
            //}
            //else //There is at least one instanceOf edge.
            //{
            //    //Nodes that have P31 as a property
            //    var instanceOfNodes = graph.nodes.Where(n => instanceOfEdges.Select(e => e.sourceId).Contains(n.id));

            //    //Properties First
            //    if (!graph.selected.isNode)
            //    {
            //        var selectedEdge = graph.edges.FirstOrDefault(e => graph.selected.id.Equals(e.id));

            //        //Edge ?prop going from sourceNode
            //        var sourceNode = graph.nodes.FirstOrDefault(n => n.id.Equals(selectedEdge.sourceId));
            //        //Edge going to targetNode
            //        var targetNode = graph.nodes.FirstOrDefault(x => x.id.Equals(selectedEdge.targetId));

            //        var sourceNodeIsKnownType = instanceOfNodes.Any(x=>x.id.Equals(sourceNode.id));
            //        var targetNodeIsKnownType = instanceOfNodes.Any(x=>x.id.Equals(targetNode.id));

            //        if (sourceNodeIsKnownType && targetNodeIsKnownType)
            //            return QueryGraphType.KnownSubjectAndObjectTypesIntersectDomainRangeProperties;
            //        if (sourceNodeIsKnownType)
            //            return QueryGraphType.KnownSubjectTypeOnlyQueryDomainProperties;
            //        if (targetNodeIsKnownType)
            //            return QueryGraphType.KnownObjectTypeOnlyQueryRangeProperties;

            //        return QueryGraphType.Unkwown;
            //    }
            //    else
            //    {
            //        // These are the cases where I am querying for the nodes of known types.
            //        // Meaning instances of (human -> Obama) this is something that I am not currently returning, but I could.
            //        // Also this is I think where Aidan wants us to infer the type.

            //        //var selectedNode = graph.nodes.FirstOrDefault(e => graph.selected.id.Equals(e.id));
            //        ////The selectedNode is kwown?
            //        //var selectedNodeIsKnownType = instanceOfNodes.Any(x=>x.id.Equals(selectedNode.id));
                    
            //        ////Now I would like to know if the nodes targeting this node or the ones that this node targets are known
            //        ////selectedNode is source
            //        //var edgesFromSelectedNode = graph.edges.Where(x=>x.sourceId.Equals(selectedNode.id));
            //        //var destinationNodes = graph.nodes.Where(n=>edgesFromSelectedNode.Select(e=>e.targetId).Any(i=>i.Equals(n.id)));

            //        //var propertiesToSelectedNode = graph.edges.Where(x=>x.targetId.Equals(selectedNode.id));
                    

            //        ////Nodes that have edges to the selectedNode:
            //        //var nodesGoingToSelectedNode = graph.nodes.Where(n => graph.edges.Select(e => e.targetId).Contains(n.id));
            //    }
            //}


            return GraphQueryType.Unkwown;
        }
    }
}
