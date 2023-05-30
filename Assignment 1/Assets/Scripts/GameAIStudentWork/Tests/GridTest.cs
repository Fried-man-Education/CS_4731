using System;
using System.Collections;
using System.Collections.Generic;
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


    }
}
