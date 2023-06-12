// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;


namespace GameAICourse
{


    public class AStarPathSearchImpl
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Andrew Friedman";


        // Null Heuristic for Dijkstra
        public static float HeuristicNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }

        // Null Cost for Greedy Best First
        public static float CostNull(Vector2 nodeA, Vector2 nodeB)
        {
            return 0f;
        }



        // Heuristic distance fuction implemented with manhattan distance
        public static float HeuristicManhattan(Vector2 nodeA, Vector2 nodeB) =>
            Mathf.Abs(nodeA.x - nodeB.x) + Mathf.Abs(nodeA.y - nodeB.y);

        // Heuristic distance function implemented with Euclidean distance
        public static float HeuristicEuclidean(Vector2 nodeA, Vector2 nodeB) =>
            Mathf.Abs(Vector2.Distance(nodeA, nodeB));


        // Cost is only ever called on adjacent nodes. So we will always use Euclidean distance.
        // We could use Manhattan dist for 4-way connected grids and avoid sqrroot and mults.
        // But we will avoid that for simplicity.
        public static float Cost(Vector2 nodeA, Vector2 nodeB) =>
            Mathf.Abs(Vector2.Distance(nodeA, nodeB));



        public static PathSearchResultType FindPathIncremental(
            GetNodeCount getNodeCount,
            GetNode getNode,
            GetNodeAdjacencies getAdjacencies,
            CostCallback G,
            CostCallback H,
            int startNodeIndex, int goalNodeIndex,
            int maxNumNodesToExplore, bool doInitialization,
            ref int currentNodeIndex,
            ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
            ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
        {
            PathSearchResultType pathResult = PathSearchResultType.InProgress;

            var nodeCount = getNodeCount();

            if (startNodeIndex >= nodeCount || goalNodeIndex >= nodeCount ||
                startNodeIndex < 0 || goalNodeIndex < 0 ||
                maxNumNodesToExplore <= 0 ||
                (!doInitialization &&
                 (openNodes == null || closedNodes == null || currentNodeIndex < 0 ||
                  currentNodeIndex >= nodeCount )))

                return PathSearchResultType.InitializationError;


            // STUDENT CODE HERE - incremental search taken from BasicPathSearchImpl

            pathResult = PathSearchResultType.InProgress;

            if (doInitialization) {
                currentNodeIndex = startNodeIndex;
                searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
                var startNodeRecord = new PathSearchNodeRecord(currentNodeIndex);
                searchNodeRecords.Add(startNodeRecord.NodeIndex, startNodeRecord);
                openNodes = new SimplePriorityQueue<int, float>();
                openNodes.Enqueue(startNodeRecord.NodeIndex, 0f);
                closedNodes = new HashSet<int>();
                returnPath = new List<int>();
            }

            for (int visitedNodes = 0; visitedNodes < maxNumNodesToExplore && openNodes.Count > 0; visitedNodes++) {
                var activeNode = searchNodeRecords[openNodes.First];
                currentNodeIndex = activeNode.NodeIndex;

                if (currentNodeIndex == goalNodeIndex) break;

                PathSearchNodeRecord adjacentNode = null;
                var adjacentEdges = getAdjacencies(currentNodeIndex);

                foreach (var adjacentIndex in adjacentEdges) {
                    var pathCost = activeNode.CostSoFar +
                        G(getNode(currentNodeIndex), getNode(adjacentIndex));
                    float heuristic = 0f;

                    if (closedNodes.Contains(adjacentIndex)) {
                        adjacentNode = searchNodeRecords[adjacentIndex];
                        if (adjacentNode.CostSoFar <= pathCost) continue;
                        closedNodes.Remove(adjacentIndex);
                        heuristic = adjacentNode.EstimatedTotalCost - adjacentNode.CostSoFar;
                    } else if (openNodes.Contains(adjacentIndex)) {
                        adjacentNode = searchNodeRecords[adjacentIndex];
                        if (adjacentNode.CostSoFar <= pathCost) continue;
                        heuristic = adjacentNode.EstimatedTotalCost - adjacentNode.CostSoFar;
                    } else {
                        adjacentNode = new PathSearchNodeRecord(adjacentIndex);
                        heuristic = H(getNode(currentNodeIndex), getNode(adjacentIndex));
                    }

                    adjacentNode.FromNodeIndex = currentNodeIndex;
                    adjacentNode.CostSoFar = pathCost;
                    adjacentNode.EstimatedTotalCost = pathCost + heuristic;
                    searchNodeRecords[adjacentIndex] = adjacentNode;

                    if (!openNodes.Contains(adjacentIndex))
                        openNodes.Enqueue(adjacentIndex, heuristic);
                }

                openNodes.Remove(currentNodeIndex);
                closedNodes.Add(currentNodeIndex);
            }


            if (openNodes.Count > 0 || currentNodeIndex == goalNodeIndex) {
                pathResult = currentNodeIndex == goalNodeIndex 
                    ? PathSearchResultType.Complete 
                    : PathSearchResultType.InProgress;
            }
            else {
                pathResult = PathSearchResultType.Partial;
                int nearestNodeId = -1;
                float closestDistance = float.MaxValue;

                foreach (int nodeId in closedNodes) {
                    float currentDistance = Vector2.Distance(getNode(searchNodeRecords[nodeId].NodeIndex), getNode(goalNodeIndex));

                    if (currentDistance < closestDistance) {
                        nearestNodeId = nodeId;
                        closestDistance = currentDistance;
                    }
                }
                
                currentNodeIndex = nearestNodeId >= 0 
                    ? nearestNodeId 
                    : currentNodeIndex;
            }

            if (pathResult != PathSearchResultType.InProgress) {
                Stack<int> tempPath = new Stack<int>();
                for (int currentNode = currentNodeIndex; currentNode != startNodeIndex; currentNode = searchNodeRecords[currentNode].FromNodeIndex)
                    tempPath.Push(currentNode);
                tempPath.Push(startNodeIndex);

                while (tempPath.Count > 0)
                    returnPath.Add(tempPath.Pop());
            }

            return pathResult;
        }

    }

}