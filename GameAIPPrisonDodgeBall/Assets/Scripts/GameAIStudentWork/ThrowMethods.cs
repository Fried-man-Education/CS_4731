// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GameAIStudent
{

    public class ThrowMethods
    {

        public const string StudentName = "Andrew Friedman";


        // Note: You have to implement the following method with prediction:
        // Either directly solved (e.g. Law of Cosines or similar) or iterative.
        // You cannot modify the method signature. However, if you want to do more advanced
        // prediction (such as analysis of the navmesh) then you can make another method that calls
        // this one. 
        // Be sure to run the editor mode unit test to confirm that this method runs without
        // any gamemode-only logic
        public static bool PredictThrow(
            // The initial launch position of the projectile
            Vector3 projectilePos,
            // The initial ballistic speed of the projectile
            float maxProjectileSpeed,
            // The gravity vector affecting the projectile (likely passed as Physics.gravity)
            Vector3 projectileGravity,
            // The initial position of the target
            Vector3 targetInitPos,
            // The constant velocity of the target (zero acceleration assumed)
            Vector3 targetConstVel,
            // The forward facing direction of the target. Possibly of use if the target
            // velocity is zero
            Vector3 targetForwardDir,
            // For algorithms that approximate the solution, this sets a limit for how far
            // the target and projectile can be from each other at the interceptT time
            // and still count as a successful prediction
            float maxAllowedErrorDist,
            // Output param: The solved projectileDir for ballistic trajectory that intercepts target
            out Vector3 projectileDir,
            // Output param: The speed the projectile is launched at in projectileDir such that
            // there is a collision with target. projectileSpeed must be <= maxProjectileSpeed
            out float projectileSpeed,
            // Output param: The time at which the projectile and target collide
            out float interceptT,
            // Output param: An alternate time at which the projectile and target collide
            // Note that this is optional to use and does NOT coincide with the solved projectileDir
            // and projectileSpeed. It is possibly useful to pass on to an incremental solver.
            // It only exists to simplify compatibility with the ShootingRange
            out float altT)
        {
            projectileSpeed = maxProjectileSpeed * 0.9f;
            Vector3 vectorProj = new Vector3(projectilePos.x, projectilePos.y, projectilePos.z), 
                vectorInit = new Vector3(targetInitPos.x, targetInitPos.y, targetInitPos.z),
                diff = (vectorProj - vectorInit),
                velTemp = targetConstVel;

            float diffMagazine = diff.magnitude,
                velMagazine = velTemp.magnitude,
                projMagazine = projectileSpeed,
                thetiana = Vector3.Dot(Vector3.Normalize(diff), Vector3.Normalize(velTemp));

            interceptT = diff.magnitude / maxProjectileSpeed;
            altT = -1f;

            projectileDir = targetConstVel.x == 0 && targetConstVel.y == 0 && targetConstVel.z == 0 ? targetForwardDir : velTemp;

            float init = Mathf.Pow(projMagazine, 2) - Mathf.Pow(velMagazine, 2),
                enit = 2f * diffMagazine * velMagazine * thetiana,
                unit = Mathf.Pow(diffMagazine, 2);

            float temp = Mathf.Pow(enit, 2) + 4f * init * unit;
            if (temp < 0 || 2 * init == 0) return false;

            if ((-enit + Mathf.Sqrt(temp)) / (2 * init) < 0) {
                if ((-enit - Mathf.Sqrt(temp)) / (2 * init) < 0) {
                    return false;
                } else {
                    interceptT = (-enit - Mathf.Sqrt(temp)) / (2 * init);
                }
            } else {
                interceptT = (-enit - Mathf.Sqrt(temp)) / (2 * init) < 0 ? (-enit + Mathf.Sqrt(temp)) / (2 * init) : Mathf.Min((-enit + Mathf.Sqrt(temp)) / (2 * init), (-enit - Mathf.Sqrt(temp)) / (2 * init));
            }

            Vector3 fin = ((vectorInit - vectorProj) / interceptT) + velTemp - (0.5f * projectileGravity * interceptT);
            if(fin.magnitude > maxProjectileSpeed) return false;
            projectileSpeed = fin.magnitude;
            projectileDir = Vector3.Normalize(fin);

            return true;
        }
    }

}