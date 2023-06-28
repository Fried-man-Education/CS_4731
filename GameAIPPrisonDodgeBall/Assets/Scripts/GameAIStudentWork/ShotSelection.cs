// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GameAIStudent
{

    public class ShotSelection
    {

        public const string StudentName = "Andrew Friedman";


        public enum SelectThrowReturn
        {
            DoThrow,
            NoThrowTargettingFailed,
            NoThrowOpponentCurrentlyAccelerating,
            NoThrowOpponentWillAccelerate,
            NoThrowOpponentOccluded
        }

        public static SelectThrowReturn SelectThrow(
                // the minion doing the throwing, can also be used to query generic params true of all minions
                MinionScript thisMinion,
                // info about the target
                PrisonDodgeballManager.OpponentInfo opponent,
                // What is the navmask that defines where on the navmesh the opponent can traverse
                int opponentNavmask,
                // typically this is a value a tiny bit smaller than the radius of minion added with radius of the dodgeball
                float maxAllowedThrowErrDist,
                // Output param: The solved projectileDir for ballistic trajectory that intercepts target
                out Vector3 projectileDir,
                // Output param: The speed the projectile is launched at in projectileDir such that
                // there is a collision with target. projectileSpeed must be <= maxProjectileSpeed
                out float projectileSpeed,
                // Output param: The time at which the projectile and target collide
                out float interceptT,
                // Output param: where the shot is expected to hit
                out Vector3 interceptPos
            )
        {
            var Mgr = PrisonDodgeballManager.Instance;


            var opponentVel = opponent.Vel; // Or perhaps use thisMinion.MaxPathSpeed (max speed a minion can go)
                                            // times dir if you think minion is nearly there.
                                            // Using something other than the opponent's current Vel requires extra logic

            interceptPos = opponent.Pos;
            bool predThrow = ThrowMethods.PredictThrow(
                thisMinion.HeldBallPosition, 
                thisMinion.ThrowSpeed, 
                Physics.gravity, 
                opponent.Pos, 
                opponentVel, 
                opponent.Forward, 
                maxAllowedThrowErrDist, 
                out projectileDir, 
                out projectileSpeed, 
                out interceptT,
                out float altT
            );
            if (!predThrow) return SelectThrowReturn.NoThrowTargettingFailed;
            interceptPos = opponent.Pos + opponent.Vel * interceptT;

            Vector3 diff = interceptPos - thisMinion.HeldBallPosition,
                temp = (opponentVel - opponent.PrevVel) / Time.deltaTime;
            if(temp.magnitude > 1.3) return SelectThrowReturn.NoThrowOpponentCurrentlyAccelerating;

            if (Mathf.Abs(Vector3.Angle(opponent.PrevForward, opponent.Forward)) > 0.35) return SelectThrowReturn.NoThrowOpponentCurrentlyAccelerating;

            NavMeshHit spaceballs;
            if(NavMesh.Raycast(opponent.Pos, interceptPos, out spaceballs, opponentNavmask)) return SelectThrowReturn.NoThrowOpponentWillAccelerate;

            int carverMask = ~(1 << Mgr.NavMeshCarverLayerIndex);

            int minionMask = ~(1 << Mgr.MinionTeamBLayerIndex) & ~(1 << Mgr.MinionTeamALayerIndex);
            int ballMask = ~(1 << Mgr.BallTeamALayerIndex) & ~(1 << Mgr.BallTeamBLayerIndex);
            int mask = Physics.AllLayers & carverMask & ballMask & minionMask;

            return Physics.Raycast(
                    thisMinion.HeldBallPosition - 0.25f * Vector3.Cross(Vector3.Normalize(diff), Vector3.up), Vector3.Normalize(projectileDir), diff.magnitude, mask) || 
                    Physics.Raycast(thisMinion.HeldBallPosition + 0.25f * Vector3.Cross(Vector3.Normalize(diff), Vector3.up), Vector3.Normalize(projectileDir), diff.magnitude, mask
                ) ? 
                    SelectThrowReturn.NoThrowOpponentOccluded : 
                    SelectThrowReturn.DoThrow;
        }
    }
}