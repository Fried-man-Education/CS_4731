using System.Linq;
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
        public static object[] OwnTestCases =
        {
            new TestCaseData(
                new[]
                {
                    new[] { 1 },
                    new[] { 2 },
                    new[] { 3 },
                    new[] { 4 },
                    new[] { 5 },
                    new[] { 6 },
                    new[] { 7 },
                    new[] { 8 }
                },
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                PathSearchResultType.Complete,
                28.13432091141765f),
            new TestCaseData(new[]
                {
                    new[] { 1, 2, 3 },
                    new[] { 2, 5, 6 },
                    new[] { 3, 4, 5 },
                    new[] { 4, 7 },
                    new[] { 5, 8 },
                    new[] { 6, 7 },
                    new[] { 7, 8 },
                    new[] { 8, 6 }
                }, new[] { 0, 2, 4, 8 },
                PathSearchResultType.Complete,
                8.142353110033618f),
            new TestCaseData(new[]
                {
                    new[] { 1, 4 },
                    new[] { 0, 4, 6 },
                    new[] { 0, 1, 6, 7 },
                    new[] { 2, 4, 5 },
                    new[] { 2, 3, 5 },
                    new[] { 4, 7, 8 },
                    new[] { 4, 7, 8 },
                    new[] { 4, 8 }
                }, new[] { 0, 4, 5, 8 },
                PathSearchResultType.Complete,
                11.14004830889689f),
            new TestCaseData(new[]
                {
                    new[] { 1, 4 },
                    new[] { 0, 4, 6 },
                    new[] { 0, 1, 6, 7 },
                    new[] { 2, 4, 5 },
                    new[] { 2, 3, 5 },
                    new[] { 4, 7 },
                    new[] { 4, 7 },
                    new[] { 4 }
                }, new[] { 0, 4, 2 },
                PathSearchResultType.Partial,
                6.64950943f),
            new TestCaseData(new[]
                {
                    new[] { 1, 3 },
                    new[] { 5 },
                    new[] { 1, 3, 6 },
                    new[] { 6, 7 },
                    new[] { 5, 7 },
                    new[] { 7 },
                    new[] { 4 },
                    new[] { 1, 3, 4, 5, 6 }
                }, new[] { 0, 3, 7, 4 },
                PathSearchResultType.Partial,
                14.906578772007919f),
        };

        [TestCaseSource(nameof(OwnTestCases))]
        public void TestFindPath(int[][] edgesArray, int[] expectedReturnPath,
            PathSearchResultType expectedResultType, float expectedCost)
        {
            Vector2[] nodesArray =
            {
                new(0.0f, 0.0f),
                new(1.1f, 5.2f),
                new(2.4f, 1.9f),
                new(3.5f, 4.9f),
                new(4.1f, 0.0f),
                new(1.2f, 0.0f),
                new(0.0f, 3.0f),
                new(0.1f, 2.4f),
                new(4.5f, 2.5f)
            };

            var nodes = new List<Vector2>(nodesArray);
            var edges = edgesArray.Select(eArray => new List<int>(eArray)).ToList();

            var currentNodeIndex = 0;
            Dictionary<int, PathSearchNodeRecord> searchNodeRecords = null;
            Priority_Queue.SimplePriorityQueue<int, float> openNodes = null;
            HashSet<int> closedNodes = null;
            List<int> returnPath = null;

            var goalNodeIndex = nodesArray.Length - 1;
            var resultType = AStarPathSearchImpl.FindPathIncremental(
                () => nodes.Count,
                (nindex) => nodes[nindex],
                (nindex) => edges[nindex],
                AStarPathSearchImpl.Cost, AStarPathSearchImpl.HeuristicEuclidean,
                0, goalNodeIndex, int.MaxValue, true,
                ref currentNodeIndex, ref searchNodeRecords, ref openNodes, ref closedNodes,
                ref returnPath);

            Assert.That(resultType, Is.EqualTo(expectedResultType));
            Assert.That(returnPath.ToArray(), Is.EqualTo(expectedReturnPath));
            var finalNodeRecord = searchNodeRecords[returnPath[^1]];
            Assert.That(finalNodeRecord.CostSoFar, Is.EqualTo(expectedCost).Within(0.0001));
        }



        [TestCase(true)]
        [TestCase(false)]
        public void StartIsGoal(bool incrementalSearch)
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
            var goalNode = 0;

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
            Assert.That(returnPath.Count, Is.EqualTo(1));
            Assert.That(returnPath[0], Is.EqualTo(startNode));
            Assert.That(attempts, Is.EqualTo(1));

        }

        [TestCase(true)]
        [TestCase(false)]
        public void StartToGoalOffset(bool incrementalSearch)
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

            var startNode = 1;
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

            if (incrementalSearch)
                Assert.That(attempts, Is.EqualTo(5));

            Assert.That(ret, Is.EqualTo(PathSearchResultType.Complete));
            Assert.That(returnPath, Does.Contain(goalNode));
            Assert.That(returnPath, !Does.Contain(0));
            Assert.That(returnPath[0], Is.EqualTo(startNode));
            Assert.That(returnPath, Is.EqualTo(new int[] { 1, 2, 3, 4, 5 }));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void StartToGoalWithMaximumAttemptsZero(bool incrementalSearch)
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
            var maxNumNodesToExplore = 1;
            int currentNodeIndex = 0;

            Dictionary<int, PathSearchNodeRecord> searchNodeRecord = null;
            Priority_Queue.SimplePriorityQueue<int, float> openNodes = null;
            HashSet<int> closedNodes = null;
            List<int> returnPath = null;

            var ret = PathSearchResultType.InProgress;

            int attempts = 0;

            int maxAllowedAttempts = 0;

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

            Assert.That(attempts, Is.EqualTo(1));
            Assert.That(ret, Is.EqualTo(PathSearchResultType.InProgress));
            Assert.That(returnPath, Is.Null);
            Assert.That(searchNodeRecord.Count, Is.EqualTo(2));
            Assert.That(searchNodeRecord.Keys, Is.EqualTo(new int[] { 0, 1 }));
            Assert.That(openNodes, Is.EqualTo(new int[] { 1 }));
            Assert.That(closedNodes, Is.EqualTo(new int[] { 0 }));
        }

        [Test]
        public void ClosestNodeWithGoalUnreachable()
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
                    new int[] {0 },         //1
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

            var maxNumNodesToExplore = int.MaxValue;// incrementalSearch ? 1 : int.MaxValue;

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

            Assert.That(ret, Is.EqualTo(PathSearchResultType.Partial));
            Assert.That(returnPath, !Does.Contain(goalNode));
            Assert.That(returnPath.Count, Is.EqualTo(2));
            Assert.That(returnPath, Is.EqualTo(new int[] { 0, 1 }));
        }


        [Test]
        public void HandGcallback()
        {
            Vector2[] _nodes = new Vector2[] {
                    new Vector2(-5.0f, -5.0f), //0
                    new Vector2(5.0f, 5.0f), //1

                };

            CostCallback G = AStarPathSearchImpl.Cost;
            CostCallback H = AStarPathSearchImpl.HeuristicEuclidean;

            float dH = H(_nodes[0], _nodes[1]);
            float dG = G(_nodes[0], _nodes[1]);

            Assert.That(dH, Is.Not.Negative);
            Assert.That(dH, Is.EqualTo(14.1421356237).Within(0.0001));
            Assert.That(dH, Is.EqualTo(dG));
        }

        [Test]
        public void Mcallback()
        {
            Vector2[] _nodes = new Vector2[] {
                    new Vector2(-5.0f, -5.0f), //0
                    new Vector2(5.0f, 5.0f), //1
                    };

            CostCallback M = AStarPathSearchImpl.HeuristicManhattan;

            float dM = M(_nodes[0], _nodes[1]);

            Assert.That(dM, Is.Not.Negative);
            Assert.That(dM, Is.EqualTo(20));

        }


        [Test]
        public void DoInitialization()
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
                        new int[] {2, 0},       //1
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
            var maxNumNodesToExplore = 2;
            int currentNodeIndex = 0;

            Dictionary<int, PathSearchNodeRecord> searchNodeRecord = null;
            Priority_Queue.SimplePriorityQueue<int, float> openNodes = null;
            HashSet<int> closedNodes = null;
            List<int> returnPath = null;

            var ret = PathSearchResultType.InProgress;
            int attempts = 0;
            int maxAllowedAttempts = 2;

            do
            {
                //var init = attempts <= 0;
                if (attempts > 0)
                    maxNumNodesToExplore = 1;

                ++attempts;


                ret = AStarPathSearchImpl.FindPathIncremental(
                    () => { return nodes.Count; },
                    (nindex) => { return nodes[nindex]; },
                    (nindex) => { return edges[nindex]; },
                    G, H,
                    startNode, goalNode, maxNumNodesToExplore, true,
                    ref currentNodeIndex, ref searchNodeRecord, ref openNodes, ref closedNodes,
                    ref returnPath);
                //maxNumNodesToExplore = -99999999;
            }
            while (ret == PathSearchResultType.InProgress && attempts < maxAllowedAttempts);

            Debug.Log($"Number of updates: {attempts}");

            Assert.That(ret, Is.EqualTo(PathSearchResultType.InProgress));
            Assert.That(searchNodeRecord.Count, Is.EqualTo(2));
            Assert.That(openNodes.Count, Is.EqualTo(1));
            Assert.That(openNodes, Does.Contain(1));
            Assert.That(closedNodes.Count, Is.EqualTo(1));
            Assert.That(closedNodes, Does.Contain(0));
            Assert.That(attempts, Is.EqualTo(2));
        }
    }
}
