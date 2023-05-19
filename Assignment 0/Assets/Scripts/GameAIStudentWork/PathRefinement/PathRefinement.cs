using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAICourse
{

    public class PathRefinement
    {

        // Please change this string to your name
        public const string StudentAuthorName = "George P. Burdell ← Not your name, change it!";


        public static bool Refine(Vector2 canvasOrigin, float canvasWidth, float canvasHeight, float agentRadius,
            List<Vector2> path, List<Obstacle> obstacles, int numPasses, int numLineBisections, out List<Vector2> refinedPath)
        {

            // TODO STUDENT CODE HERE

            refinedPath = new List<Vector2>();

            refinedPath.AddRange(path);

            return true;

            // END STUDENT CODE
        }

    }

}