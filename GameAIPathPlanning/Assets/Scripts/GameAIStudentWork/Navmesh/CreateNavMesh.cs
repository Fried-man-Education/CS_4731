// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameAICourse
{


    public class CreateNavMesh
    {

        public static string StudentAuthorName = "Andrew Friedman";



        // Helper method provided to help you implement this file. Leave as is.
        // Converts Vector2 to Vector2Int with default factor for computational geometry (1000)
        static public Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (but not on edge) the polygon defined by pts (CCW winding). False, otherwise
        public static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {

            return CG.InPoly1(pts, p) == CG.PointPolygonIntersectionType.Inside;

        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if there is at least one intersection between A and a polygon in polys
        public static bool IntersectsConvexPolygons(Polygon A, List<Polygon> polys)
        {
            return CG.IntersectionConvexPolygons(A, polys);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Tests to see if AB is an edge in a list of polys
        public static bool IsLineSegmentInPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
        {
            return CG.IsLineSegmentInPolygons(A, B, polys);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Tests if abc are collinear
        static public bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return CG.Collinear(a, b, c);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Tests if the polygon winding is CCW
        static public bool IsCCW(Vector2Int[] poly)
        {
            return CG.Ccw(poly);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Tests if C is between A and B (first tests if C is collinear with A and B
        // and then whether C is on the line between A and B
        static public bool Between(Vector2Int a, Vector2Int b, Vector2Int c)
        {
            return CG.Between(a, b, c);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Tests if AB intersects with the interior of any poly of polys (touching the outside of a poly does not
        // count an intersection)
        public static bool InteriorIntersectionLineSegmentWithPolygons(Vector2Int A, Vector2Int B, List<Polygon> polys)
        {
            return CG.InteriorIntersectionLineSegmentWithPolygons(A, B, polys);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Merges two polygons into one across a common edge AB/BA
        public static Polygon MergePolygons(Polygon poly1, Polygon poly2, Vector2Int A, Vector2Int B)
        {
            return Utils.MergePolygons(poly1, poly2, A, B);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Tests if a poly is convex
        static public bool IsConvex(Vector2Int[] poly)
        {
            return CG.CheckForConvexity(poly);
        }



        // Create(): Creates a navmesh and path graph (associated with navmesh) 
        // canvasOrigin: bottom left corner of navigable region in world coordinates
        // canvasWidth: width of navigable region in world dimensions
        // canvasHeight: height of navigable region in world dimensions
        // obstacles: a list of Polygons that are obstacles in the scene
        // agentRadius: the radius of the agent
        // offsetObst: out param of the complex expanded obstacles for visualization purposes
        // origTriangles: out param of the triangles that are used for navmesh generation
        //          These triangles are passed out for validation and visualization.
        // navmeshPolygons: out param of the convex polygons of the navmesh (list). 
        //          These polys are passed out for validation and visualization
        // adjPolys: out param of type AdjacentPolygons. These are used validation
        // pathNodes: a list of graph nodes (either centered on portal edges or navmesh polygon, depending
        //                        on assignment requirements)
        // pathEdges: graph adjacency list for each node. Cooresponding index of pathNodes match
        //      a node with its edge list. All nodes must have an edge list (no null list)
        //      entries in each edge list are indices into pathNodes
        // 
        public static void Create(
        Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles, float agentRadius,
            out List<Polygon> offsetObst,
            out List<Polygon> origTriangles,
            out List<Polygon> navmeshPolygons,
            out AdjacentPolygons adjPolys,
            out List<Vector2> pathNodes,
            out List<List<int>> pathEdges
            )
        {

            // Some basic initialization
            pathEdges = new List<List<int>>();
            pathNodes = new List<Vector2>();

            origTriangles = new List<Polygon>();
            navmeshPolygons = null;

            // This is a special dictionary for tracking polygons that share
            // edges. It is going to be used to determine which triangles can be merged
            // into larger convex polygons. Later, it will also be used for generating
            // the pathNetwork on top of the navmesh
            adjPolys = new AdjacentPolygons();

            // Holds the complex set of polys representing obstacle boundaries
            // Any time you need to test against obstacles, use offsetObstPolys
            // instead of obstacles
            List<Polygon> offsetObstPolys;

            // This creates a complex set of polygons representing the obstacle boundaries.
            // It's built with a 3rd party library called Clipper. In addition
            // to finding the union of obstacle boundaries, and clipping against the canvas, 
            // it also performs expansion for agentOffset
            Utils.GenerateOffsetNavSpace(canvasOrigin, canvasWidth, canvasHeight,
               agentRadius, obstacles, out offsetObstPolys);


            List<Polygon> tmp = new List<Polygon>(offsetObstPolys);

            // We currently don't support holes, but we can remove them. Holes
            // might form from union of polys, or (more rarely) expansion of concave polys.
            // There could be a hole with another poly inside, possibly repeating recursively.
            // In this case, removing holes will leave overlapping polys, but this shouldn't have
            // any bad effect other than wasted computation.
            foreach (var p in tmp)
            {
                if (!IsCCW(p.getIntegerPoints()))
                {
                    Debug.Log("*** Removed a hole from obstacles! ***");
                    offsetObstPolys.Remove(p);
                }
            }

            offsetObst = offsetObstPolys; // out param for viz

            // Obtain all the vertices that are going to be used to form our triangles
            List<Vector2Int> obstacleVertices;
            Utils.AllVerticesFromPolygons(offsetObstPolys, out obstacleVertices);

            // Let's also add the four corners of the canvas (with offset)
            var A = canvasOrigin + new Vector2(agentRadius, agentRadius);
            var B = canvasOrigin + new Vector2(0f, canvasHeight) + new Vector2(agentRadius, -agentRadius);
            var C = canvasOrigin + new Vector2(canvasWidth, canvasHeight) + new Vector2(-agentRadius, -agentRadius);
            var D = canvasOrigin + new Vector2(canvasWidth, 0f) + new Vector2(-agentRadius, agentRadius);

            var Ai = Convert(A);
            var Bi = Convert(B);
            var Ci = Convert(C);
            var Di = Convert(D);

            obstacleVertices.Add(Ai);
            obstacleVertices.Add(Bi);
            obstacleVertices.Add(Ci);
            obstacleVertices.Add(Di);


            // ******************** PHASE 0 - Change your name string ************************
            // TODO set your name above

            //********************* PHASE I - Brute force triangle formation *****************

            // In this phase, some scaffolding is provided for you. Your goal to to produce
            // triangles that will serve as the foundation of your navmesh. You will use
            // a brute force method of evaluating all combinations of three vertices to see
            // if a valid triangle is formed. This includes checking for degenerate triangles, 
            // triangles that intersect obstacle boundaries, and triangles that intersect
            // triangles you already made. There is also a special test to see if triangles
            // break adjacency (described later).

            // Iterate through combinations of obstacle vertices that can form triangle
            // candidates.
            var obstVertCount = obstacleVertices.Count;
            for (int i = 0; i < obstVertCount - 2; ++i)
            {

                for (int j = i + 1; j < obstVertCount - 1; ++j)
                {

                    for (int k = j + 1; k < obstVertCount; ++k)
                    {
                        // These are vertices for a candidate triangle
                        // that we hope to form
                        var V1 = obstacleVertices[i];
                        var V2 = obstacleVertices[j];
                        var V3 = obstacleVertices[k];

                        if (IsCollinear(V1, V2, V3)) continue;

                        bool obstacleEdge1 = IsLineSegmentInPolygons(V1, V2, offsetObstPolys);
                        bool obstacleEdge2 = IsLineSegmentInPolygons(V1, V3, offsetObstPolys);
                        bool obstacleEdge3 = IsLineSegmentInPolygons(V2, V3, offsetObstPolys);

                        bool inBetween = false;
                        for (int p = 0; p < obstacleVertices.Count - 2; ++p) 
                        {
                            Vector2Int vertex = obstacleVertices[p];
                            if ((!obstacleEdge1 && vertex != V1 && vertex != V2 && Between(V1, V2, vertex)) ||
                                (!obstacleEdge2 && vertex != V1 && vertex != V3 && Between(V1, V3, vertex)) ||
                                (!obstacleEdge3 && vertex != V2 && vertex != V3 && Between(V2, V3, vertex)))
                            {
                                inBetween = true;
                                break;
                            }
                        }
                        if (inBetween) continue;

                        Polygon candidateTri = new Polygon();
                        candidateTri.SetIntegerPoints(new Vector2Int[] { V1, V2, V3 });
                        if (!IsCCW(candidateTri.getIntegerPoints())) candidateTri.Reverse();

                        if (IntersectsConvexPolygons(candidateTri, origTriangles)) continue;

                        bool pointInside = false, equalsPolygon = false;

                        for (int p = 0; p < obstacleVertices.Count; ++p)
                        {
                            if (IsPointInsidePolygon(candidateTri.getIntegerPoints(), obstacleVertices[p]))
                            {
                                pointInside = true;
                                break;
                            }
                        }

                        for (int p = 0; p < offsetObstPolys.Count; ++p)
                        {
                            if (candidateTri.Equals(offsetObstPolys[p]))
                            {
                                equalsPolygon = true;
                                break;
                            }
                        }

                        if (pointInside || equalsPolygon) continue;

                        if ((!obstacleEdge1 && InteriorIntersectionLineSegmentWithPolygons(V1, V2, offsetObstPolys)) ||
                            (!obstacleEdge2 && InteriorIntersectionLineSegmentWithPolygons(V1, V3, offsetObstPolys)) ||
                            (!obstacleEdge3 && InteriorIntersectionLineSegmentWithPolygons(V2, V3, offsetObstPolys))) continue;

                        origTriangles.Add(candidateTri);
                        adjPolys.AddPolygon(candidateTri);
                    }
                }
            }

            // Priming the navmeshPolygons for next steps, and also allow visualization
            navmeshPolygons = new List<Polygon>(origTriangles);

            AdjacentPolygons updatedAdjPolys = new AdjacentPolygons(adjPolys);
            

            bool mergesOccur;
            do {
                mergesOccur = false;
                foreach (var edge in adjPolys.Keys) {
                    var tempPolys = adjPolys[edge];
                    if (tempPolys.IsBarrier || tempPolys.AB == null || tempPolys.BA == null) 
                        continue;

                    var newPolygon = MergePolygons(tempPolys.AB, tempPolys.BA, edge.A, edge.B);
                    if (!IsConvex(newPolygon.getIntegerPoints())) 
                        continue;

                    updatedAdjPolys.Remove(edge);
                    updatedAdjPolys.AddPolygon(newPolygon, tempPolys.AB, tempPolys.BA);
                    navmeshPolygons.Remove(tempPolys.AB);
                    navmeshPolygons.Remove(tempPolys.BA);
                    navmeshPolygons.Add(newPolygon);
                    mergesOccur = true;
                }
                adjPolys = updatedAdjPolys;
            } while (mergesOccur);

            Dictionary<Polygon, int> polygonDict = new Dictionary<Polygon, int>();
            int counter = 0;
            foreach (Polygon polygon in navmeshPolygons) {
                polygonDict[polygon] = counter;
                pathNodes.Add(polygon.GetCentroid());
                pathEdges.Add(new List<int>());
                counter++;
            }

            foreach (var edge in adjPolys.Keys) {
                var tempPolys = adjPolys[edge];
                if (tempPolys.IsBarrier) 
                    continue;

                pathEdges[polygonDict[tempPolys.AB]].Add(polygonDict[tempPolys.BA]);
                pathEdges[polygonDict[tempPolys.BA]].Add(polygonDict[tempPolys.AB]);
            }
        } // Create()


    }

}