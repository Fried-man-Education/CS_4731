using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using GameAICourse;

namespace Tests
{
    public class GridTest
    {
        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(CreateGrid.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }


        // You can write helper methods that are called by multiple tests!
        // This method is not itself a test because it is not annotated with [Test].
        // But look below for examples of calling it.
        void BasicGridCheck(bool[,] grid, float width, float height, float cellSize)
        {
            Assert.That(grid, Is.Not.Null);
            Assert.That(grid.Rank, Is.EqualTo(2), "grid is not a 2D array!");

            var w = grid.GetLength(0);
            var h = grid.GetLength(1);

            // Parameterized tests can be dangerous. Especially if you implement
            // an equation to generate the correct values. This may replicate
            // bugs in the code that you are testing and give a false
            // indication of correctness!
            // Be very cautious when doing this...
            Assert.That(w, Is.EqualTo(Mathf.FloorToInt(width / cellSize)));
            Assert.That(h, Is.EqualTo(Mathf.FloorToInt(height / cellSize)));

        }


        // You can write parameterized tests for more efficient test coverage!
        // This single method can reflect an arbitrary number of test configurations
        // via the TestCase(...) syntax.
        // TODO You probably want some more test cases here. Maybe a negative origin?
        [TestCase(0f, 0f, 1f, 1f, 1f)]
        [TestCase(0f, 0f, 1f, 1f, 0.25f)]
        [TestCase(0f, 0f, 1f, 1f, 0.26f)]
        public void TestEmptyGrid(float originx, float originy, float width, float height, float cellSize)
        {

            var origin = new Vector2(originx, originy);

            bool[,] grid;

            List<Polygon> obstPolys = new List<Polygon>();


            // Here is an example of testing code you are working on by calling it!
            CreateGrid.Create(origin, width, height, cellSize, obstPolys, out grid);


            // Herer is that helper method in action
            BasicGridCheck(grid, width, height, cellSize);


            Assert.That(grid, Has.All.True,
                "There aren't any obstacles to block the grid cells!");

            // TODO This method can be extended with more rigorous testing...

        }


        [TestCase(0f, 0f, 1f, 1f, 1f)]
        [TestCase(0f, 0f, 1f, 1f, 0.25f)]
        public void TestObstacleThatNearlyFillsCanvas(float originx, float originy,
            float width, float height, float cellSize)
        {

            var origin = new Vector2(originx, originy);

            bool[,] grid;
 
            List<Polygon> obstPolys = new List<Polygon>();

            // Let's make an obstacle in this test...

            Polygon poly = new Polygon();

            float offset = 0.1f;

            // Needs to be counterclockwise!
            Vector2[] pts =
                {
                    origin + Vector2.one * offset,
                    origin + new Vector2(width - offset, offset),
                    origin + new Vector2(width - offset, height - offset),
                    origin + new Vector2(offset, height-offset)
                };

            // There are different ways to approach test setup for tests.
            // I generally just assert things that I believe might be problematic.
            // I then add text like "SETUP FAILURE" so I know the problem is separate
            // from what I'm actually testing.

            Assert.That(CG.Ccw(pts), Is.True, "SETUP FAILURE: polygon verts not listed CCW");

            poly.SetPoints(pts);

            obstPolys.Add(poly);


            // Here is an example of testing code you are working on!
            CreateGrid.Create(origin, width, height, cellSize, obstPolys, out grid);

     
            BasicGridCheck(grid, width, height, cellSize);

            Assert.That(grid, Has.All.False,
                "There is a big obstacle that should have blocked the entire grid!");

            // TODO This method can be extended with more rigorous testing...

        }

        // This test checks the functionality of your IsTraversable() method.
        // It's very important that this method works correctly. You will
        // need to test it a lot more than just this simple example test.
        [TestCase(true)]
        [TestCase(false)]
        public void TestTraversableWithSingleGridCell(bool gridCellState)
        {
            bool[,] grid = new bool[1, 1];

            grid[0, 0] = gridCellState;

            // Test all possible directions
            foreach (var dir in (TraverseDirection[])Enum.GetValues(typeof(TraverseDirection)))
            {          
                var res = CreateGrid.IsTraversable(grid, 0, 0, dir);
                Assert.That(res, Is.False, $"Traverability in dir: {dir} expected to be false but wasn't");
            }
        }


        // TODO I bet there is a lot more you want to write tests for!
        [TestCase(0, 0, TraverseDirection.Up, true)]
        [TestCase(0, 0, TraverseDirection.Down, false)]
        [TestCase(0, 0, TraverseDirection.Left, false)]
        [TestCase(0, 0, TraverseDirection.UpLeft, false)]
        [TestCase(0, 0, TraverseDirection.DownLeft, false)]
        [TestCase(1, 3, TraverseDirection.DownLeft, true)]
        [TestCase(1, 3, TraverseDirection.Left, true)]
        [TestCase(1, 3, TraverseDirection.UpLeft, false)]
        [TestCase(1, 3, TraverseDirection.Up, false)]
        [TestCase(1, 3, TraverseDirection.UpRight, false)]
        [TestCase(1, 3, TraverseDirection.Right, true)]
        [TestCase(1, 3, TraverseDirection.Down, false)]
        [TestCase(1, 3, TraverseDirection.DownRight, false)]
        [TestCase(1, 1, TraverseDirection.Left, false)]
        [TestCase(6, 1, TraverseDirection.Left, false)]
        [TestCase(6, 1, TraverseDirection.DownLeft, false)]
        [TestCase(6, 1, TraverseDirection.Down, true)]
        [TestCase(6, 1, TraverseDirection.DownRight, true)]
        [TestCase(6, 1, TraverseDirection.Right, true)]
        [TestCase(6, 1, TraverseDirection.UpRight, true)]
        [TestCase(6, 1, TraverseDirection.Up, true)]
        [TestCase(6, 1, TraverseDirection.UpLeft, true)]
        [TestCase(8, 3, TraverseDirection.Left, true)]
        [TestCase(8, 3, TraverseDirection.DownLeft, true)]
        [TestCase(8, 3, TraverseDirection.Down, true)]
        [TestCase(8, 3, TraverseDirection.DownRight, false)]
        [TestCase(8, 3, TraverseDirection.Right, false)]
        [TestCase(8, 3, TraverseDirection.UpRight, false)]
        [TestCase(8, 3, TraverseDirection.Up, false)]
        [TestCase(8, 3, TraverseDirection.UpLeft, false)]
        public void TestIsTraversable(int x, int y, TraverseDirection dir, bool expected)
        {
            bool[,] grid;
            List<Polygon> obstPolys = new List<Polygon>();
            Polygon poly = new Polygon();
            poly.SetPoints(new Vector2[] {
                new(-1.5f, -0.5f),
                new(-0.4f, -1f),
                new(1f, -0.5f),
                new(-1.5f, 0.5f),
            });
            obstPolys.Add(poly);
            CreateGrid.Create(new Vector2(-2f, -1f), 4.5f, 2f, 0.5f, obstPolys, out grid);
            var actual = CreateGrid.IsTraversable(grid, x, y, dir);
            Assert.That(actual, Is.EqualTo(expected));
        }
        
        public static object[] ObstaclesCases =
        {
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(0.25f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 4f), new Vector2(0.25f, 4f) }, new bool[,] { { false, false, false, false, true }, { true, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true } }).SetName("Vertical line thinner than grid cell"),
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f), new Vector2(1f, 1f) }, new bool[,] { { true, true, true, true, true }, { false, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true } }).SetName("Obstacle size and placement same as grid cell"),
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(0f, 0f), new Vector2(5f, 0f), new Vector2(5f, 5f), new Vector2(0f, 5f) }, new bool[,] { { false, false, false, false, false }, { false, false, false, false, false }, { false, false, false, false, false }, { false, false, false, false, false }, { false, false, false, false, false } }).SetName("Obstacle covers entire grid"),
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(2.25f, 1f), new Vector2(2.5f, 2.5f), new Vector2(2.25f, 4.01f) }, new bool[,] { { true, true, true, true, true }, { true, true, true, true, true }, { true, false, false, false, false }, { true, true, true, true, true }, { true, true, true, true, true } }).SetName("Triangle with one barely intersecting point and one barely overlapping point"),
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(0.25f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(0.25f, 0.5f) }, new bool[,] { { false, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true }, { true, true, true, true, true } }).SetName("Obstacle contained within a single grid cell"),
            new TestCaseData(0f, 0f, 5f, 5f, 1f, new Vector2[] { new Vector2(2f, 2.5f), new Vector2(3.5f, 3.5f), new Vector2(4.5f, 2.5f), new Vector2(4f, 3.5f), new Vector2(4.75f, 4f), new Vector2(4f, 4.5f), new Vector2(3.5f, 4.75f), new Vector2(3f, 4.5f), new Vector2(1.5f, 4f), new Vector2(2f, 3f) }, new bool[,] { { true, true, true, true, true }, { true, true, true, false, false }, { true, true, false, false, false }, { true, true, true, false, false }, { true, true, false, false, false } }).SetName("The ugliest star you\'ve ever seen")
        };

        [TestCaseSource(nameof(ObstaclesCases))]
        public void TestMoreObstacles(float originx, float originy,
            float width, float height, float cellSize, Vector2[] obstaclePoints, bool[,] correctGrid)
        {

            var origin = new Vector2(originx, originy);

            bool[,] grid;

            List<Polygon> obstPolys = new List<Polygon>();

            Polygon poly = new Polygon();

            Vector2[] pts = obstaclePoints;

            Assert.That(CG.Ccw(pts), Is.True, "SETUP FAILURE: polygon verts not listed CCW");

            poly.SetPoints(pts);
            obstPolys.Add(poly);
            CreateGrid.Create(origin, width, height, cellSize, obstPolys, out grid);

            Debug.Log("Obstacle is at: " + $"[{string.Join(",", obstaclePoints)}]");
            PrintGrid(grid, "Returned");
            PrintGrid(correctGrid, "Correct");

            BasicGridCheck(grid, width, height, cellSize);

            Assert.That(grid, Is.EqualTo(correctGrid), $"Grids are not equal");

        }

        public void PrintGrid(bool[,] grid, string gridType)
        {
            StringBuilder sb = new StringBuilder();
            for (int y = grid.GetLength(1) - 1; y >= 0; y--)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    sb.Append(grid[x, y]);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            Debug.Log(gridType + " values are as follows: \n" + sb.ToString());
        }

        [Test]
        public void TestTraversableAllDirections()
        {
            bool[,] grid = new bool[3, 3];

            grid[0, 0] = true;
            grid[0, 1] = false;
            grid[0, 2] = true;
            grid[1, 0] = false;
            grid[1, 1] = true;
            grid[1, 2] = true;
            grid[2, 0] = false;
            grid[2, 1] = true;
            grid[2, 2] = false;

            // Test all possible directions
            
            var downRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.Down);
            Assert.That(downRes, Is.False, $"Traverability in dir: {TraverseDirection.Down} expected to be false but wasn't");

            var upRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.Up);
            Assert.That(upRes, Is.True, $"Traverability in dir: {TraverseDirection.Up} expected to be true but wasn't");

            var leftRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.Left);
            Assert.That(leftRes, Is.False, $"Traverability in dir: {TraverseDirection.Left} expected to be false but wasn't");

            var rightRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.Right);
            Assert.That(rightRes, Is.True, $"Traverability in dir: {TraverseDirection.Right} expected to be true but wasn't");

            var upLeftRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.UpLeft);
            Assert.That(upLeftRes, Is.True, $"Traverability in dir: {TraverseDirection.UpLeft} expected to be true but wasn't");

            var upRightRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.UpRight);
            Assert.That(upRightRes, Is.False, $"Traverability in dir: {TraverseDirection.UpRight} expected to be false but wasn't");

            var downLeftRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.DownLeft);
            Assert.That(downLeftRes, Is.True, $"Traverability in dir: {TraverseDirection.DownLeft} expected to be true but wasn't");

            var downRightRes = CreateGrid.IsTraversable(grid, 1, 1, TraverseDirection.DownRight);
            Assert.That(downRightRes, Is.False, $"Traverability in dir: {TraverseDirection.DownRight} expected to be false but wasn't");

        }
    }
}
