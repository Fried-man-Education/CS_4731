using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
public class GreedySimplePathSearchImpl
{
    public static PathSearchResultType FindPathIncremental(
        //List<Vector2> nodes,
        //List<List<int>> edges,
        GetNodeCount getNodeCount,
        GetNode getNode,
        GetNodeAdjacencies adjacencies,
        int startNodeIndex, int goalNodeIndex, int maxNumNodesToExplore, bool doInitialization, ref int currentNodeIndex, ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords, ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
    {
        //Simple-greedy search doesn't leverage any meta data during its search, however we will still populate
        //open and closed node lists for visualization purposes
        var pathResult = PathSearchResultType.InProgress;

        var nodeCount = getNodeCount();

        if (
            //nodes == null ||
            startNodeIndex >= nodeCount || goalNodeIndex >= nodeCount ||
            //edges == null || startNodeIndex >= edges.Count || goalNodeIndex >= edges.Count ||
            //edges.Count != nodes.Count ||
            startNodeIndex < 0 || goalNodeIndex < 0 ||
            maxNumNodesToExplore <= 0 ||
            (!doInitialization &&
             (openNodes == null || closedNodes == null || currentNodeIndex < 0 ||
              currentNodeIndex >= nodeCount
              //|| currentNodeIndex >= edges.Count
              )))
        {
            searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
            openNodes = new SimplePriorityQueue<int, float>();
            closedNodes = new HashSet<int>();

            return PathSearchResultType.InitializationError;
        }

        if (doInitialization)
        {
            currentNodeIndex = startNodeIndex;
            searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
            openNodes = new SimplePriorityQueue<int, float>();
            closedNodes = new HashSet<int>();
            var firstNodeRecord = new PathSearchNodeRecord(currentNodeIndex); 
            searchNodeRecords.Add(firstNodeRecord.NodeIndex, firstNodeRecord);
            openNodes.Enqueue(firstNodeRecord.NodeIndex, 0f);
            returnPath = new List<int>();
        }

        int nodesProcessed = 0;
        while (nodesProcessed < maxNumNodesToExplore && currentNodeIndex != goalNodeIndex)
        {
            ++nodesProcessed;
            returnPath.Add(currentNodeIndex);
            openNodes.Remove(currentNodeIndex);
            closedNodes.Add(currentNodeIndex);
            //var currEdges = edges[currentNodeIndex];
            var currEdges = adjacencies(currentNodeIndex);
            var minDist = float.MaxValue;
            var minIndex = -1;
            foreach (var edgeEndIndex in currEdges)
            {
                var dist = Vector2.Distance(getNode(edgeEndIndex), getNode(goalNodeIndex));
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndex = edgeEndIndex;
                }
                
                var nrec = new PathSearchNodeRecord(edgeEndIndex, currentNodeIndex);
                // if we pick one, we will revert the add to closed and move it to open later...

                searchNodeRecords[nrec.NodeIndex] =  nrec;

                closedNodes.Add(nrec.NodeIndex);
            }
            var currDistToGoal = Vector2.Distance(getNode(currentNodeIndex), getNode(goalNodeIndex));

            if ((minIndex < 0) || (minDist >= currDistToGoal))
            {
                pathResult = PathSearchResultType.Partial;
                break;
            }
            // fix the overzealous add to closed from above, and move it to open instead...
            // Not the most effecient way to do it, but simple greedy isn't very useful to begin with...
            closedNodes.Remove(minIndex);
            openNodes.Enqueue(minIndex, 0f);

            currentNodeIndex = minIndex;
  
        } //while

        if(currentNodeIndex == goalNodeIndex)
        {
            pathResult = PathSearchResultType.Complete;
            openNodes.Remove(currentNodeIndex);
            returnPath.Add(currentNodeIndex);
        }
  
        return pathResult;
    }
}
