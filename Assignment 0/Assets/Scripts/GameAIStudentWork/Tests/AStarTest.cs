using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using GameAICourse;

namespace Tests
{
    public class AStarTest
    {
        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(AStarPathSearchImpl.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }


        [TestCase(true)]
        [TestCase(false)]
        public void BasicTest(bool incrementalSearch)
        {

            Vector2[] _nodes = new Vector2[] {
                new Vector2(0.0f, 0.0f), //0
                new Vector2(0.0f, 1.0f), //1
                new Vector2(0.0f, 2.0f), //2
                new Vector2(0.0f, 3.0f), //3
                new Vector2(0.0f, 4.0f), //4
                new Vector2(0.0f, 5.0f), //5
            };
            int[][] _edges = new int[][] {
                new int[] {1 },         //0
                new int[] {2, 0 },      //1
                new int[] {3, 1 },      //2
                new int[] {4, 2 },      //3
                new int[] {5, 3 },      //4
                new int[] {4 }          //5
            };

            List<Vector2> nodes = new List<Vector2>(_nodes);
            List<List<int>> edges = new List<List<int>>();

            foreach (var eArray in _edges)
            {
                var elist = new List<int>(eArray);
                edges.Add(elist);
            }

            CostCallback G = AStarPathSearchImpl.Cost;
            CostCallback H = AStarPathSearchImpl.HeuristicEuclidean;

            var startNode = 0;
            var goalNode = nodes.Count - 1;

            var maxNumNodesToExplore = incrementalSearch ? 1 : int.MaxValue;

            int currentNodeIndex = 0;

            Dictionary<int, PathSearchNodeRecord> searchNodeRecord = null;

            Priority_Queue.SimplePriorityQueue<int, float> openNodes = null;

            HashSet<int> closedNodes = null;

            List<int> returnPath = null;

            var ret = PathSearchResultType.InProgress;

            int attempts = 0;

            int maxAllowedAttempts = 20;

            do
            {
                var init = attempts <= 0;

                ++attempts;

                ret = AStarPathSearchImpl.FindPathIncremental(
                    () => { return nodes.Count; },
                    (nindex) => { return nodes[nindex]; },
                    (nindex) => { return edges[nindex]; },
                    G, H,
                    startNode, goalNode, maxNumNodesToExplore, init,
                    ref currentNodeIndex, ref searchNodeRecord, ref openNodes, ref closedNodes,
                    ref returnPath);
            }
            while (ret == PathSearchResultType.InProgress && attempts < maxAllowedAttempts);

            Debug.Log($"Number of updates: {attempts}");

            Assert.That(ret, Is.EqualTo(PathSearchResultType.Complete));
            Assert.That(returnPath, Does.Contain(goalNode));

            if (incrementalSearch)
                Assert.That(attempts, Is.GreaterThan(1));
            else
                Assert.That(attempts, Is.EqualTo(1));

            // TODO write some good assertions as this test is incomplete

        }

        // TODO write more tests!

    }
}
