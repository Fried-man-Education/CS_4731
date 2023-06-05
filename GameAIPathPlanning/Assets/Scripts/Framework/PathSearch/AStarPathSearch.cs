using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

using GameAICourse;



public delegate float CostCallback(Vector2 a, Vector2 b);



public class AStarPathSearch : PathSearchProvider
{

    private static PathSearchProvider instance;
    public static PathSearchProvider Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AStarPathSearch();
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

        CostCallback h;

        if (variant.HasFlag(PathSearchVariantType.ManhattanDistance))
            h = AStarPathSearchImpl.HeuristicManhattan;
        else
            h = AStarPathSearchImpl.HeuristicEuclidean;

        return AStarPathSearchImpl.FindPathIncremental(
            //nodes,
            //edges,
            getNodeCount,
            getNode,
            adjacencies,
            AStarPathSearchImpl.Cost, h, startNodeIndex, goalNodeIndex, maxNumNodesToExplore, doInitialization,
            ref currentNodeIndex, ref searchNodeRecords, ref openNodes, ref closedNodes, ref returnPath);
            
    }
}
