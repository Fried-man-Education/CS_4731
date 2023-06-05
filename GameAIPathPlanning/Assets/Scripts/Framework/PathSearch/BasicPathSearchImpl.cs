using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
public class BasicPathSearchImpl
{
    static float G(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }
    public static PathSearchResultType FindPathIncremental(
       //List<Vector2> nodes,
       //List<List<int>> edges,
       GetNodeCount getNodeCount,
       GetNode getNode,
       GetNodeAdjacencies adjacencies,
       int startNodeIndex, int goalNodeIndex,
       bool IsBFS, //true for BFS, false for DFS,
       bool randomExpansion, // true if expanded edges added in random order
       int maxNumNodesToExplore, bool doInitialization,
       ref int currentNodeIndex,
       ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
       ref SimplePriorityQueue<int, float> openNodes, ref HashSet<int> closedNodes, ref List<int> returnPath)
    {
        var nodeCount = getNodeCount();
        PathSearchResultType pathResult = PathSearchResultType.InProgress;
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

        //float max_dfs_priority = Mathf.Pow(2f, 20f);

        if (doInitialization)
        {
            currentNodeIndex = startNodeIndex;
            searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
            openNodes = new SimplePriorityQueue<int, float>();
            closedNodes = new HashSet<int>();
            var firstNodeRecord = new PathSearchNodeRecord(currentNodeIndex);
            searchNodeRecords.Add(firstNodeRecord.NodeIndex, firstNodeRecord);
            //float startingPriority = IsBFS ? 0f : max_dfs_priority;
            float startingPriority = 0f;
            openNodes.Enqueue(firstNodeRecord.NodeIndex, startingPriority);
            returnPath = new List<int>();
        }

        float currentPriority = 0f;

        if (openNodes.Count > 0)
            currentPriority = openNodes.GetPriority(openNodes.First);

        // DFS priorities go negative so our priority queue always gives the most recently explored nodes
        // we take ABS here to minimize changes to logic elsewhere in the code
        currentPriority = Mathf.Abs(currentPriority);

        int nodesProcessed = 0;
        while (nodesProcessed < maxNumNodesToExplore && openNodes.Count > 0)
        {
            //Find the smallest element in the open list using the estimated total cost
            var currentNodeRecord = searchNodeRecords[openNodes.First]; 
            currentNodeIndex = currentNodeRecord.NodeIndex;

   
            ++nodesProcessed;

            // goal check should be in edge expansion for better time performance!
            // However, doing so means goal found sooner. But if we are using DFS
            // we are probably intentionally looking for long paths...
            if (currentNodeIndex == goalNodeIndex)
                break;


            bool updateOpen = false;

            // In range [0.0, 1.0]
            // if DFS with randomExpansion, smaller value results in tighter
            // path. Larger value more meandering path.
            float updateOpenThreshold = 0.5f;


            PathSearchNodeRecord edgeNodeRecord = null;

            //var currEdges = edges[currentNodeIndex];
            var currEdges = adjacencies(currentNodeIndex);

            // Just for fun. If random expansion is enabled, DFS can generate funny paths
            if (randomExpansion)
            {
                List<int> shuffleEdges = new List<int>(currEdges);

                currEdges = new List<int>(shuffleEdges.Count);

                while(shuffleEdges.Count > 0)
                {
                    int i = Random.Range(0, shuffleEdges.Count);
                    currEdges.Add(shuffleEdges[i]);
                    shuffleEdges.RemoveAt(i);
                }
            }
            
   
            foreach (var edgeNodeIndex in currEdges)
            {
                var costToEdgeNode = currentNodeRecord.CostSoFar +
                    G(getNode(currentNodeIndex), getNode(edgeNodeIndex));


                if (closedNodes.Contains(edgeNodeIndex))
                {
                    continue;
                }
                else if(openNodes.Contains(edgeNodeIndex))
                {

                    if (!IsBFS)
                    {
                        if (randomExpansion)
                            updateOpen = Random.value > updateOpenThreshold;
                        else
                            updateOpen = true;
                    }


                    if (updateOpen)
                        edgeNodeRecord = searchNodeRecords[edgeNodeIndex];
                    else
                        continue;
                }
                else
                {
                    edgeNodeRecord = new PathSearchNodeRecord(edgeNodeIndex);      
                }

                currentPriority += 1;

                edgeNodeRecord.FromNodeIndex = currentNodeIndex;
         
                searchNodeRecords[edgeNodeIndex] = edgeNodeRecord;

                // This simple trick allows this code to support both BFS and DFS.
                var newPriority = IsBFS ? currentPriority : -currentPriority;

                if (!openNodes.Contains(edgeNodeIndex))
                {
                    openNodes.Enqueue(edgeNodeIndex, newPriority);
                }
                else
                {
                    if (IsBFS)
                        Debug.LogError("Shouldn't rewrite open set in BFS!");

                    if(updateOpen)
                        openNodes.UpdatePriority(edgeNodeIndex, newPriority);

                }

            } //foreach() edge processing of current node

            openNodes.Remove(currentNodeIndex);
            closedNodes.Add(currentNodeIndex);
        } //while
        if (openNodes.Count <= 0 && currentNodeIndex != goalNodeIndex)
        {
            pathResult = PathSearchResultType.Partial;
            //find the closest node we looked at and use for partial path
            int closest = -1;
            float closestDist = float.MaxValue;
            foreach (var n in closedNodes)
            {
                var nrec = searchNodeRecords[n];

                // Hmmm. I bet if we were using this partial path code with an A* implementation 
                // we could avoid calculating Euclidean distance again using cached metadata.
                // (But we would need to deduce whether said code was running in Dijkstra
                // mode with a zero constant Heuristic() func)
                // Otherwise, we must calculate the distance
                var d = Vector2.Distance(getNode(nrec.NodeIndex), getNode(goalNodeIndex));
                if (d < closestDist)
                {
                    closest = n;
                    closestDist = d;
                }
            }
            if (closest >= 0)
            {
                currentNodeIndex = closest;
            }
        }
        else if (currentNodeIndex == goalNodeIndex)
        {
            pathResult = PathSearchResultType.Complete;
        }


        if (pathResult != PathSearchResultType.InProgress)
        {
            // processing complete, a path (possibly partial) can be generated returned
            returnPath = new List<int>();
            while (currentNodeIndex != startNodeIndex)
            {
                returnPath.Add(currentNodeIndex);
                currentNodeIndex = searchNodeRecords[currentNodeIndex].FromNodeIndex;
            }
            returnPath.Add(startNodeIndex);
            returnPath.Reverse();
        }

        return pathResult;

    }
}
