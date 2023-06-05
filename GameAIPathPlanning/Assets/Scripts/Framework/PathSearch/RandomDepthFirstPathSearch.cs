using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
public class RandomDepthFirstPathSearch : PathSearchProvider
{
    private static PathSearchProvider instance;
    public static PathSearchProvider Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RandomDepthFirstPathSearch();
            }
            return instance;
        }
    }
    override public PathSearchResultType FindPathIncremental(
        //List<Vector2> nodes,
        //List<List<int>> edges,
        GetNodeCount getNodeCount,
        GetNode getNode,
        GetNodeAdjacencies adjacencies,
        PathSearchVariantType variant,
        int startNodeIndex, int goalNodeIndex, int maxNumNodesToExplore, bool doInitialization, ref int currentNodeIndex, ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords, ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
    {

        return BasicPathSearchImpl.FindPathIncremental(
            //nodes,
            //edges,
            getNodeCount,
            getNode,
            adjacencies,
            startNodeIndex, goalNodeIndex, false, true, maxNumNodesToExplore, doInitialization,
            ref currentNodeIndex, ref searchNodeRecords, ref openNodes, ref closedNodes, ref returnPath);

    }
}
