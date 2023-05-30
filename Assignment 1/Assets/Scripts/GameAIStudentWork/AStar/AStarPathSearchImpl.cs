// compile_check
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
        public const string StudentAuthorName = "George P. Burdell ← Not your name, change it!";


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
        public static float HeuristicManhattan(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return 0f;

            //END CODE 
        }

        // Heuristic distance function implemented with Euclidean distance
        public static float HeuristicEuclidean(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return 0f;

            //END CODE 
        }


        // Cost is only ever called on adjacent nodes. So we will always use Euclidean distance.
        // We could use Manhattan dist for 4-way connected grids and avoid sqrroot and mults.
        // But we will avoid that for simplicity.
        public static float Cost(Vector2 nodeA, Vector2 nodeB)
        {
            //STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation
            return 0f;

            //END STUDENT CODE
        }



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


            // STUDENT CODE HERE

            // The following code is just a placeholder so that the method has a valid return
            // You will replace it with the correct implementation

            pathResult = PathSearchResultType.Complete;

            searchNodeRecords = new Dictionary<int, PathSearchNodeRecord>();
            openNodes = new SimplePriorityQueue<int, float>();
            closedNodes = new HashSet<int>();

            returnPath = new List<int>();

            returnPath.Add(startNodeIndex);

            return pathResult;

            //END STUDENT CODE HERE
        }

    }

}