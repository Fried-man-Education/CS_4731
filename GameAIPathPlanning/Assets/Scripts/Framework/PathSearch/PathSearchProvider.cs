using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
public class PathSearchProvider : IPathSearchProvider
{
    virtual public PathSearchResultType FindPath(
        //List<Vector2> nodes,
        //List<List<int>> edges,
        GetNodeCount getNodeCount,
        GetNode getNode,
        GetNodeAdjacencies adjacencies,
        PathSearchVariantType variant, int startNodeIndex, int goalNodeIndex, ref int currentNodeIndex, ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords, ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
    {

        return FindPathIncremental(
            //nodes,
            //edges,
            getNodeCount,
            getNode,
            adjacencies,
            variant, startNodeIndex, goalNodeIndex, int.MaxValue, true, ref currentNodeIndex, ref searchNodeRecords,
            ref openNodes, ref closedNodes, ref returnPath);
    }


    virtual public PathSearchResultType FindPathIncremental(
        //List<Vector2> nodes,
        //List<List<int>> edges,
        GetNodeCount getNodeCount,
        GetNode getNode,
        GetNodeAdjacencies adjacencies,
        PathSearchVariantType variant, int startNodeIndex, int goalNodeIndex, int maxNumNodesToExplore, bool doInitialization, ref int currentNode, ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords, ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
    {
        throw new System.NotImplementedException();
    }
}
