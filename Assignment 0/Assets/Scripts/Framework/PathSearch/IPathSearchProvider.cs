using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
public interface IPathSearchProvider
{

    PathSearchResultType FindPath(
                                    //List<Vector2> nodes,
                                    //List<List<int>> edges,
                                    GetNodeCount getNodeCount,
                                    GetNode getNode,
                                    GetNodeAdjacencies adjacencies,
                                    PathSearchVariantType variant,
                                    int startNodeIndex,
                                    int goalNodeIndex,                              
                                    ref int currentNode,
                                    ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
                                    ref SimplePriorityQueue<int, float> openNodes,
                                    ref HashSet<int> closedNodes,
                                    ref List<int> returnPath
                                    );


    PathSearchResultType FindPathIncremental(
                                        //List<Vector2> nodes,
                                        //List<List<int>> edges,
                                        GetNodeCount getNodeCount,
                                        GetNode getNode,
                                        GetNodeAdjacencies adjacencies,
                                        PathSearchVariantType variant,
                                        int startNodeIndex, int goalNodeIndex, 
                                        int maxNumNodesToExplore, bool doInitialization,
                                        ref int currentNode,
                                        ref Dictionary<int, PathSearchNodeRecord> searchNodeRecords,
                                        ref SimplePriorityQueue<int, float> openNodes,
                                        ref HashSet<int> closedNodes,
                                        ref List<int> returnPath
                                        );
}
