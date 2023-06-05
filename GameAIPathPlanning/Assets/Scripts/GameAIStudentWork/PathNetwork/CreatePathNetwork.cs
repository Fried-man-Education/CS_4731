// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class CreatePathNetwork
    {

        public const string StudentAuthorName = "Andrew Friedman";




        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        public static Vector2Int ConvertToInt(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        public static int ConvertToInt(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2Int converted to Vector2 according to default scaling factor (1000)
        public static Vector2 ConvertToFloat(Vector2Int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return new Vector2(v.x * f, v.y * f);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns int converted to float according to default scaling factor (1000)
        public static float ConvertToFloat(int v)
        {
            float f = 1f / (float)CG.FloatToIntFactor;
            return v * f;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static public bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2Int point, Vector2Int lineStart, Vector2Int lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        //Get the shortest distance from a point to a line
        //Line is defined by the lineStart and lineEnd points
        public static float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return CG.DistanceToLineSegment(point, lineStart, lineEnd);
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Determines if a point is inside/on a CCW polygon and if so returns true. False otherwise.
        public static bool IsPointInPolygon(Vector2Int[] polyPts, Vector2Int point)
        {
            return CG.PointPolygonIntersectionType.Outside != CG.InPoly1(polyPts, point);
        }

        // Returns true iff p is strictly to the left of the directed
        // line through a to b.
        // You can use this method to determine if 3 adjacent CCW-ordered
        // vertices of a polygon form a convex or concave angle

        public static bool Left(Vector2Int a, Vector2Int b, Vector2Int p)
        {
            return CG.Left(a, b, p);
        }

        // Vector2 version of above
        public static bool Left(Vector2 a, Vector2 b, Vector2 p)
        {
            return CG.Left(CG.Convert(a), CG.Convert(b), CG.Convert(p));
        }






        //Student code to build the path network from the given pathNodes and Obstacles
        //Obstacles - List of obstacles on the plane
        //agentRadius - the radius of the traversing agent
        //minPoVDist AND maxPoVDist - used for Points of Visibility (see assignment doc)
        //pathNodes - ref parameter that contains the pathNodes to connect (or return if pathNetworkMode is set to PointsOfVisibility)
        //pathEdges - out parameter that will contain the edges you build.
        //  Edges cannot intersect with obstacles or boundaries. Edges must be at least agentRadius distance
        //  from all obstacle/boundary line segments. No self edges, duplicate edges. No null lists (but can be empty)
        //pathNetworkMode - enum that specifies PathNetwork type (see assignment doc)

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight,
            List<Polygon> obstacles, float agentRadius, float minPoVDist, float maxPoVDist, ref List<Vector2> pathNodes, out List<List<int>> pathEdges,
            PathNetworkMode pathNetworkMode)
        {
            var totalPathNodes = pathNodes.Count;
            pathEdges = new List<List<int>>();
            Vector2Int canvasSize = ConvertToInt(new Vector2(canvasWidth, canvasHeight)),
                canvasStartPoint = ConvertToInt(canvasOrigin),
                canvasEndPoint = canvasStartPoint + canvasSize;
            int agentRadiusAsInt = ConvertToInt(agentRadius);
            var validPathNodeIndexes = new List<int>();

            for (int i = 0; i < totalPathNodes; i++)
            {
                var pathNodeAsInt = ConvertToInt(pathNodes[i]);
                var nodeRadius = new Vector2Int(agentRadiusAsInt, agentRadiusAsInt);
                var minBoundary = pathNodeAsInt - nodeRadius;
                var maxBoundary = pathNodeAsInt + nodeRadius;

                if (!(minBoundary.x < canvasStartPoint.x || minBoundary.y < canvasStartPoint.y || maxBoundary.x > canvasEndPoint.x || maxBoundary.y > canvasEndPoint.y))
                {
                    Vector2Int[] nodeBoundaries = { minBoundary, new Vector2Int(minBoundary.x, maxBoundary.y), maxBoundary, new Vector2Int(maxBoundary.x, minBoundary.y) };
                    var isNodeInsideObstacle = false;

                    for (int obstacleIndex = 0; obstacleIndex < obstacles.Count; obstacleIndex++)
                    {
                        Vector2Int[] elements = obstacles[obstacleIndex].getIntegerPoints();
                        for (int nodeBoundaryIndex = 0; nodeBoundaryIndex < nodeBoundaries.Length; nodeBoundaryIndex++)
                        {
                            if (Array.Exists(elements, point => IsPointInPolygon(elements, nodeBoundaries[nodeBoundaryIndex])))
                            {
                                isNodeInsideObstacle = true;
                                break;
                            }
                        }

                        if (isNodeInsideObstacle) break;
                    }

                    if (!isNodeInsideObstacle) validPathNodeIndexes.Add(i);
                }

                pathEdges.Add(new List<int>());
            }

            int validNodeCount = validPathNodeIndexes.Count;
            for (int nodeCounter = 0; nodeCounter < validNodeCount - 1; nodeCounter++)
            {
                for (int adjacencyCounter = nodeCounter + 1; adjacencyCounter < validNodeCount; adjacencyCounter++)
                {
                    Vector2Int
                        originNode = ConvertToInt(pathNodes[validPathNodeIndexes[nodeCounter]]),
                        destinationNode = ConvertToInt(pathNodes[validPathNodeIndexes[adjacencyCounter]]);
                    var doesEdgeIntersect = false;

                    for (int obstacleCounter = 0; obstacleCounter < obstacles.Count; obstacleCounter++)
                    {
                        Vector2Int[] obstacleVertices = obstacles[obstacleCounter].getIntegerPoints();
                        for (int obstacleVertexIndex = 0; obstacleVertexIndex < obstacleVertices.Length; obstacleVertexIndex++)
                        {
                            if (DistanceToLineSegment(obstacleVertices[obstacleVertexIndex], originNode, destinationNode) < agentRadiusAsInt)
                            {
                                doesEdgeIntersect = true;
                                break;
                            }

                            if (obstacleVertexIndex > 0 && Intersects(obstacleVertices[obstacleVertexIndex-1], obstacleVertices[obstacleVertexIndex], originNode, destinationNode))
                            {
                                doesEdgeIntersect = true;
                                break;
                            }
                        }

                        if (doesEdgeIntersect) break;
                    }

                    if (!doesEdgeIntersect)
                    {
                        pathEdges[validPathNodeIndexes[nodeCounter]].Add(validPathNodeIndexes[adjacencyCounter]);
                        pathEdges[validPathNodeIndexes[adjacencyCounter]].Add(validPathNodeIndexes[nodeCounter]);
                    }
                }
            }

        }


    }

}