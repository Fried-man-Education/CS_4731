// Remove the line above if you are subitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse {

    public class CreateGrid
    {

        // Please change this string to your name
        public const string StudentAuthorName = "Andrew Friedman";


        // Helper method provided to help you implement this file. Leave as is.
        // Returns true if point p is inside (or on edge) the polygon defined by pts (CCW winding). False, otherwise
        static bool IsPointInsidePolygon(Vector2Int[] pts, Vector2Int p)
        {
            return CG.InPoly1(pts, p) != CG.PointPolygonIntersectionType.Outside;
        }


        // Helper method provided to help you implement this file. Leave as is.
        // Returns float converted to int according to default scaling factor (1000)
        static int Convert(float v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns Vector2 converted to Vector2Int according to default scaling factor (1000)
        static Vector2Int Convert(Vector2 v)
        {
            return CG.Convert(v);
        }

        // Helper method provided to help you implement this file. Leave as is.
        // Returns true is segment AB intersects CD properly or improperly
        static bool Intersects(Vector2Int a, Vector2Int b, Vector2Int c, Vector2Int d)
        {
            return CG.Intersect(a, b, c, d);
        }


        // IsPointInsideBoundingBox(): Determines whether a point (Vector2Int:p) is On/Inside a bounding box (such as a grid cell) defined by
        // minCellBounds and maxCellBounds (both Vector2Int's).
        // Returns true if the point is ON/INSIDE the cell and false otherwise
        // This method should return true if the point p is on one of the edges of the cell.
        // This is more efficient than PointInsidePolygon() for an equivalent dimension poly
        // Preconditions: minCellBounds <= maxCellBounds, per dimension
        static bool IsPointInsideAxisAlignedBoundingBox(Vector2Int minCellBounds, Vector2Int maxCellBounds, Vector2Int p) => 
            minCellBounds.x <= p.x && p.x <= maxCellBounds.x && minCellBounds.y <= p.y && p.y <= maxCellBounds.y;




        // IsRangeOverlapping(): Determines if the range (inclusive) from min1 to max1 overlaps the range (inclusive) from min2 to max2.
        // The ranges are considered to overlap if one or more values is within the range of both.
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1 AND min2 <= max2
        static bool IsRangeOverlapping(int min1, int max1, int min2, int max2) => 
            min1 <= max2 && min2 <= max1;

        // IsAxisAlignedBouningBoxOverlapping(): Determines if the AABBs defined by min1,max1 and min2,max2 overlap or touch
        // Returns true if overlap, false otherwise.
        // Preconditions: min1 <= max1, per dimension. min2 <= max2 per dimension
        static bool IsAxisAlignedBoundingBoxOverlapping(Vector2Int min1, Vector2Int max1, Vector2Int min2, Vector2Int max2) => 
            IsRangeOverlapping(min1.x, max1.x, min2.x, max2.x) && IsRangeOverlapping(min1.y, max1.y, min2.y, max2.y);





        // IsTraversable(): returns true if the grid is traversable from grid[x,y] in the direction dir, false otherwise.
        // The grid boundaries are not traversable. If the grid position x,y is itself not traversable but the grid cell in direction
        // dir is traversable, the function will return false.
        // returns false if the grid is null, grid rank is not 2 dimensional, or any dimension of grid is zero length
        // returns false if x,y is out of range
        // Note: public methods are autograded
        public static bool IsTraversable(bool[,] grid, int x, int y, TraverseDirection dir)
        {
            if (
                grid == null || 
                grid.Rank != 2 || 
                grid.GetLength(0) == 0 || 
                grid.GetLength(1) == 0 ||
                x < 0 || 
                x >= grid.GetLength(0) || 
                y < 0 || 
                y >= grid.GetLength(1) ||
                grid[x, y] == false
            )
                return false;

            int maxGridWidthIndex = grid.GetLength(0) - 1; int maxGridHeightIndex = grid.GetLength(1) - 1;
            switch (dir)
            {
                case TraverseDirection.Up:
                    if (y + 1 <= maxGridHeightIndex) 
                        return grid[x, y + 1];
                    break;

                case TraverseDirection.Down:
                    if (y - 1 >= 0) 
                        return grid[x, y - 1];
                    break;

                case TraverseDirection.UpLeft:
                    if (x - 1 >= 0 && y + 1 <= maxGridHeightIndex) 
                        return grid[x - 1, y + 1];
                    break;

                case TraverseDirection.UpRight:
                    if (x + 1 <= maxGridWidthIndex && y + 1 <= maxGridHeightIndex)
                        return grid[x + 1, y + 1];
                    break;

                case TraverseDirection.DownLeft:
                    if (x - 1 >= 0 && y - 1 >= 0)
                        return grid[x - 1, y - 1];
                    break;

                case TraverseDirection.DownRight:
                    if (x + 1 <= maxGridWidthIndex && y - 1 >= 0)
                        return grid[x + 1, y - 1];
                    break;

                case TraverseDirection.Left:
                    if (x - 1 >= 0) 
                        return grid[x - 1, y];
                    break;

                case TraverseDirection.Right:
                    if (x + 1 <= maxGridWidthIndex) 
                        return grid[x + 1, y];
                    break;
            }

            return false;
        }


        // Create(): Creates a grid lattice discretized space for navigation.
        // canvasOrigin: bottom left corner of navigable region in world coordinates
        // canvasWidth: width of navigable region in world dimensions
        // canvasHeight: height of navigable region in world dimensions
        // cellWidth: target cell width (of a grid cell) in world dimensions
        // obstacles: a list of collider obstacles
        // grid: an array of bools. A cell is true if navigable, false otherwise
        //    Example: grid[x_pos, y_pos]

        public static void Create(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float cellWidth,
            List<Polygon> obstacles,
            out bool[,] grid
            )
        {
            // ignoring the obstacles for this limited demo; 
            // Marks cells of the grid untraversable if geometry intersects interior!
            // Carefully consider all possible geometry interactions

            // also ignoring the world boundary defined by canvasOrigin and canvasWidth and canvasHeight
            static bool[,] InitializeGrid(int columns, int rows)
            {
                bool[,] grid = new bool[columns, rows];

                if (columns <= 0 || rows <= 0) return grid;

                int currentCell = 0;
                while (currentCell < columns * rows)
                    grid[currentCell % columns, currentCell++ / columns] = true;

                return grid;
            }

            static bool IsCellTraversable(List<Polygon> obstacles, float cellWidth, int row, int column, Vector2 canvasOrigin)
            {
                float offsetY = cellWidth * column + canvasOrigin[0], offsetX = cellWidth * row + canvasOrigin[1];

                Vector2Int[] corners = new Vector2Int[]
                {
                    new Vector2Int(Convert(offsetY) + 1, Convert(offsetX) + 1),
                    new Vector2Int(Convert(offsetY) + 1, Convert(cellWidth + offsetX) - 1),
                    new Vector2Int(Convert(cellWidth + offsetY) - 1, Convert(offsetX) + 1),
                    new Vector2Int(Convert(cellWidth + offsetY) - 1, Convert(cellWidth + offsetX) - 1)
                };

                foreach (Polygon obstacle in obstacles) 
                {
                    Vector2[] points = obstacle.getPoints();
                    Vector2Int[] obsCorners = new Vector2Int[points.Length];

                    for (int i = 0; i < points.Length; i++)
                    {
                        obsCorners[i] = Convert(points[i]);

                        if (IsPointInsideAxisAlignedBoundingBox(corners[0], corners[3], obsCorners[i]))
                            return false;
                    }

                    foreach (Vector2Int corner in corners)
                        if (IsPointInsidePolygon(obsCorners, corner))
                            return false;

                    for (int i = 0; i < obsCorners.Length; i++)
                        for (int j = 0; j < 4; j++)
                            if (Intersects(corners[j], corners[(j + 1) % 4], obsCorners[i], obsCorners[(i + 1) % obsCorners.Length]))
                                return false;
                }

                return true;
            }

            int columns = Mathf.FloorToInt(canvasWidth / cellWidth), rows = Mathf.FloorToInt(canvasHeight / cellWidth);
            grid = InitializeGrid(columns, rows);
            
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    grid[j, i] = IsCellTraversable(
                        obstacles, 
                        cellWidth, 
                        i, 
                        j, 
                        canvasOrigin
                    );
                }
            }
        }
    }
}