using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using GameAICourse;

namespace Tests
{
    public class NavmeshTest
    {
        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (but not on edge) the polygon defined by pts (CCW winding). False, otherwise
        public static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {

            return CG.InPoly1(pts, p) == CG.PointPolygonIntersectionType.Inside;

        }

        public static bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return CG.Collinear(a, b, c);
        }

        // Applies a small 8-connected jitter in every point
        // This may be used for richer tests
        public static void jitter(List<Vector2> points, float delta,
            out List<Vector2> pointsJitter)
        {
            pointsJitter = new List<Vector2>();
            
            foreach (var pt in points)
            {
                pointsJitter.Add(new Vector2(pt.x, pt.y));
                pointsJitter.Add(new Vector2(pt.x - delta, pt.y));
                pointsJitter.Add(new Vector2(pt.x + delta, pt.y));
                pointsJitter.Add(new Vector2(pt.x, pt.y + delta));
                pointsJitter.Add(new Vector2(pt.x, pt.y - delta));
                pointsJitter.Add(new Vector2(pt.x - delta, pt.y - delta));
                pointsJitter.Add(new Vector2(pt.x + delta, pt.y - delta));
                pointsJitter.Add(new Vector2(pt.x - delta, pt.y + delta));
                pointsJitter.Add(new Vector2(pt.x + delta, pt.y + delta));
            }
        }

        // You can run the tests in this class in the Unity Editor if you open
        // the Test Runner Window and choose the EditMode tab


        // Annotate methods with [Test] or [TestCase(...)] to create tests like this one!
        [Test]
        public void TestName()
        {
            // Tests are performed through assertions. You can Google NUnit Assertion
            // documentation to learn about them. Several examples follow.
            Assert.That(CreateNavMesh.StudentAuthorName, Is.Not.Contains("George P. Burdell"),
                "You forgot to change to your name!");
        }



        // TODO Write some tests! See GridTest and PathNetworkTest for more examples
        // PathNetworkTest(s) you have previously worked on will be useful
        // to validate you are creating a good path graph from your navmesh!


        // Check if the NavMesh is incorrectly blocking adjacencies via common edges.
        // This test does not check for merged triangles nor Path Network.
        [Test]
        public void TestTrianglesForBlockedAdjancies()
        {
            // Set up some parameters for testing
            float cellSize = 1f;
            Vector2 origin = new Vector2(0f, 0f);

            // Create 4 squared cells:      - -
            //                             | |x|
            //                              - -
            //                             | | |
            //                              - -
            Vector2 size = new Vector2(2 * cellSize, 2 * cellSize);
            float agentRadius = 0f;
            List<Polygon> obstacles = new List<Polygon>();
            
            // Block the top-right cell
            Polygon mPoly = new Polygon();
            mPoly.SetPoints(new Vector2[] {
                new Vector2(  cellSize,   cellSize),
                new Vector2(2*cellSize,   cellSize),
                new Vector2(2*cellSize, 2*cellSize),
                new Vector2(  cellSize, 2*cellSize)});
            obstacles.Add(mPoly);

            // Output params
            List<Polygon> offsetObst;
            List<Polygon> origTriangles;
            List<Polygon> navmeshPolygons;
            AdjacentPolygons adjPolys;
            List<Vector2> pathNodes;
            List<List<int>> pathEdges;


            // Create the NavMesh (your code)!

            CreateNavMesh.Create(origin, size.x, size.y, obstacles, agentRadius,
                out offsetObst, out origTriangles, out navmeshPolygons,
                out adjPolys, out pathNodes, out pathEdges);

                
            // foreach (var tri in origTriangles)
            // {
            //     Debug.Log("Printing NavMesh triangles coordinates");
            //     foreach (var vertice in tri.getPoints())
            //     {
            //         Debug.Log("Vertice x:" + vertice.x + " y:" + vertice.y);
            //     }
            // }


            // -------------------------------------------------------------------
            // Various assertions to validate your returned result

            Assert.That(origTriangles, Is.Not.Null);
            Assert.That(origTriangles, Has.Count.GreaterThan(1));

            // -------------------------------------------------------------------
            // These points below are in the middle of the obstacle so
            //   it must be out of all generated NavMesh triangles
            List<Vector2> pointsBlocked = new List<Vector2>();
            pointsBlocked.Add(new Vector2(cellSize * 1.5f, cellSize * 1.5f));
            
            List<Vector2> pointsJitterBlocked = new List<Vector2>();
            float delta = cellSize / 10; // apply a small 8-connected jitter for richer tests
            jitter(pointsBlocked, delta, out pointsJitterBlocked);

            foreach (var tri in origTriangles)
            {
                // Test if all blocked points are not inside any NavMesh triangle
                foreach (var blockedPt in pointsJitterBlocked)
                {
                    Vector2Int bPtInt = CG.Convert(blockedPt);

                    Assert.That(
                        IsPointInsidePolygon(tri.getIntegerPoints(), bPtInt),
                        Is.False,
                        "The point (" + blockedPt.x + "," + blockedPt.y +
                        ") should be blocked (outside all NavMesh triangles).");
                }
            }
            
            // -------------------------------------------------------------------
            // Test if all traversable points are inside one single NavMesh triangle
            List<Vector2> pointsTraversable = new List<Vector2>();
            // All points in the middle of these squared cells must be inside a triangle
            pointsTraversable.Add(new Vector2(cellSize *  .5f, cellSize *  .5f));
            pointsTraversable.Add(new Vector2(cellSize * 1.5f, cellSize *  .5f));
            pointsTraversable.Add(new Vector2(cellSize *  .5f, cellSize * 1.5f));

            List<Vector2> pointsJitterTraversable = new List<Vector2>();
            jitter(pointsTraversable, delta, out pointsJitterTraversable);

            foreach (var travPt in pointsJitterTraversable)
            {
                int countInside = 0;
                int countAtEdge = 0;

                foreach (var tri in origTriangles)
                {
                    Vector2Int travPtInt = CG.Convert(travPt);

                    countInside += System.Convert.ToInt32(IsPointInsidePolygon(
                        tri.getIntegerPoints(), travPtInt));                        
                        
                    Vector2Int[] vertices = tri.getIntegerPoints();
                    countAtEdge += System.Convert.ToInt32(
                        IsCollinear(vertices[0], vertices[1], travPtInt));
                    countAtEdge += System.Convert.ToInt32(
                        IsCollinear(vertices[1], vertices[2], travPtInt));
                    countAtEdge += System.Convert.ToInt32(
                        IsCollinear(vertices[2], vertices[0], travPtInt));
                }
                bool isTraversable = (countInside == 1) || (countAtEdge > 0);
                Assert.That(isTraversable, Is.True,
                    "The point (" + travPt.x + "," + travPt.y +
                    ") should be traversable (inside one single NavMesh triangle)." +
                    " countInside:" + countInside + " countAtEdge:" + countAtEdge);
            }

        }


    }
}