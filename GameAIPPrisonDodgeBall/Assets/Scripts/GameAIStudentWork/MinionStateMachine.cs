// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GameAIStudent
{

    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(MinionScript))]
    public class MinionStateMachine : MonoBehaviour
    {
        public const string StudentName = "Andrew Friedman";

        public const string GlobalTransitionStateName = "GlobalTransition";
        public const string CollectBallStateName = "CollectBall";
        public const string GoToThrowSpotStateName = "GoToThrowBall";
        public const string ThrowBallStateName = "ThrowBall";
        public const string DefensiveDemoStateName = "DefensiveDemo";
        public const string GoToPrisonStateName = "GoToPrison";
        public const string LeavePrisonStateName = "LeavePrison";
        public const string GoHomeStateName = "GoHome";
        public const string RescueStateName = "Rescue";
        public const string RestStateName = "Rest";


        // For throws...
        public static float MaxAllowedThrowPositionError = (0.25f + 0.5f) * 0.99f;

        // Data that each FSM state gets initialized with (passed as init param)
        FiniteStateMachine<MinionFSMData> fsm;

        public MinionScript Minion { get; private set; }

        PrisonDodgeballManager Mgr;
        public TeamShare TeamData { get; private set; }

        struct MinionFSMData
        {
            public MinionStateMachine MinionFSM { get; private set; }
            public MinionScript Minion { get; private set; }
            public PrisonDodgeballManager Mgr { get; private set; }
            public PrisonDodgeballManager.Team Team { get; private set; }
            public TeamShare TeamData { get; private set; }

            public MinionFSMData(
                MinionStateMachine minionFSM,
                MinionScript minion,
                PrisonDodgeballManager mgr,
                PrisonDodgeballManager.Team team,
                TeamShare teamData
                )
            {
                MinionFSM = minionFSM;
                Minion = minion;
                Mgr = mgr;
                Team = team;
                TeamData = teamData;
            }
        }






        // Simple demo of shared info amongst the team
        // You can modify this as necessary for advanced team strategy
        // Tracking teammates is added to get you started.
        // Also, some expensive queries of opponent and dodgeballs are
        // shared across the team
        public class TeamShare
        {
            public PrisonDodgeballManager.Team Team { get; private set; }
            public MinionScript[] TeamMates { get; private set; }
            public int TeamSize { get; private set; }
            public int NumBalls { get; private set; }
            protected int currTeamMateRegSpot = 0;

            // These are used to track whether data is stale
            protected float timeOfDBQuery = float.MinValue;

            protected PrisonDodgeballManager.DodgeballInfo[] dbInfo;

            public PrisonDodgeballManager.DodgeballInfo[] DBInfo
            {
                get
                {
                    var t = Time.timeSinceLevelLoad;

                    if (t != timeOfDBQuery)
                    {
                        timeOfDBQuery = t;
                        PrisonDodgeballManager.Instance.GetAllDodgeballInfo(Team, ref dbInfo, true);
                    }

                    return dbInfo;
                }

                private set { dbInfo = value; }
            }

            public TeamShare(PrisonDodgeballManager.Team team, int teamSize, int numBalls)
            {
                Team = team;
                TeamSize = teamSize;
                NumBalls = numBalls;
                TeamMates = new MinionScript[TeamSize];

                DBInfo = new PrisonDodgeballManager.DodgeballInfo[NumBalls];
            }

            public void AddTeamMember(MinionScript m)
            {
                TeamMates[currTeamMateRegSpot] = m;
                ++currTeamMateRegSpot;
            }

            public bool IsFullyInitialized
            {
                get => currTeamMateRegSpot >= TeamSize;
            }

        }

        // Create a base class for our states to have access to the parent MinionStateMachine, and other info
        // This class can be modified!
        abstract class MinionStateBase
        {
            public virtual string Name => throw new System.NotImplementedException();

            protected IFiniteStateMachine<MinionFSMData> ParentFSM;
            protected MinionStateMachine MinionFSM;
            protected MinionScript Minion;
            protected PrisonDodgeballManager Mgr;
            protected PrisonDodgeballManager.Team Team;
            protected TeamShare TeamData;


            public virtual void Init(IFiniteStateMachine<MinionFSMData> parentFSM,
                MinionFSMData minFSMData)
            {
                ParentFSM = parentFSM;
                MinionFSM = minFSMData.MinionFSM;
                Minion = minFSMData.Minion;
                Mgr = minFSMData.Mgr;
                Team = minFSMData.Team;
                TeamData = minFSMData.TeamData;
            }

            // Note: You can add extra methods here that you want to be available to all states


            protected bool FindClosestAvailableDodgeball(
                out PrisonDodgeballManager.DodgeballInfo dodgeballInfo)
            {

                var dist = float.MaxValue;
                bool found = false;

                dodgeballInfo = default;

                if (TeamData == null)
                    return false;

                var dbInfo = TeamData.DBInfo;

                if (dbInfo == null)
                    return false;

                foreach (var db in dbInfo)
                {
                    if (!db.IsHeld && db.State == PrisonDodgeballManager.DodgeballState.Neutral && db.Reachable)
                    {
                        var d = Vector3.Distance(db.Pos, Minion.transform.position);

                        if (d < dist)
                        {
                            found = true;
                            dist = d;
                            dodgeballInfo = db;
                        }

                    }
                }

                dodgeballInfo.State = PrisonDodgeballManager.DodgeballState.Team;
                return found;
            }


            public bool FindRescuableTeammate(out MinionScript firstHelplessMinion)
            {

                firstHelplessMinion = null;

                if (TeamData == null)
                    return false;

                var teammates = TeamData.TeamMates;

                if (teammates == null)
                    return false;

                foreach (var m in teammates)
                {
                    if (m == null)
                        continue;

                    if (m.CanBeRescued)
                    {
                        firstHelplessMinion = m;
                        return true;
                    }
                }
                return false;
            }


            protected void InternalEnter()
            {
                MinionFSM.Minion.DisplayText(Name);
            }

            // globalTransition parameter is to notify if transition was triggered
            // by a global transition (wildcard)
            public virtual void Exit(bool globalTransition) { }
            public virtual void Exit() { Exit(false); }

            public virtual DeferredStateTransitionBase<MinionFSMData> Update()
            {
                return null;
            }

        }

        // Create a base class for our states to have access to the parent MinionStateMachine, and other info
        abstract class MinionState : MinionStateBase, IState<MinionFSMData>
        {
            public virtual void Enter() { InternalEnter(); }
        }

        // Create a base class for our states to have access to the parent MinionStateMachine, and other info
        abstract class MinionState<S0> : MinionStateBase, IState<MinionFSMData, S0>
        {
            public virtual void Enter(S0 s) { InternalEnter(); }
        }

        // Create a base class for our states to have access to the parent MinionStateMachine, and other info
        abstract class MinionState<S0, S1> : MinionStateBase, IState<MinionFSMData, S0, S1>
        {
            public virtual void Enter(S0 s0, S1 s1) { InternalEnter(); }
        }

        // If you need MinionState<>s with more parameters (up to four total), you can add them following the pattern above

        // Go get a ball!
        class CollectBallState : MinionState
        {
            public override string Name => CollectBallStateName;

            bool hasDestBall = false;
            PrisonDodgeballManager.DodgeballInfo destBall;

            DeferredStateTransition<MinionFSMData> GoToThrowSpotTransition;
            DeferredStateTransition<MinionFSMData> DefenseDemoTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                GoToThrowSpotTransition = ParentFSM.CreateStateTransition(GoToThrowSpotStateName);
                DefenseDemoTransition = ParentFSM.CreateStateTransition(DefensiveDemoStateName);
            }

            public override void Enter()
            {
                base.Enter();

                if (FindClosestAvailableDodgeball(out destBall))
                {
                    hasDestBall = true;
                    Minion.GoTo(destBall.Pos);
                }

            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                // could pick up a ball accidentally before getting to desired ball
                if (Minion.HasBall) {
                    int index = -1;

                    if (Mgr.FindClosestNonPrisonerOpponentIndex(
                        Minion.transform.position, 
                        Team, 
                        out index
                    )) {
                        PrisonDodgeballManager.OpponentInfo info;

                        Mgr.GetOpponentInfo(Team, index, out info);

                        return ThrowMethods.PredictThrow(
                            Minion.HeldBallPosition, 
                            Minion.ThrowSpeed, 
                            Physics.gravity, 
                            info.Pos,
                            info.Vel, 
                            info.Forward, 
                            MaxAllowedThrowPositionError,
                            out var ddr, 
                            out var ss, 
                            out var inty, 
                            out var alty
                        ) ? GoToThrowSpotTransition : DefenseDemoTransition;
                    }
                }
                
                var dbInfo = TeamData.DBInfo;

                if (dbInfo == null)
                    return null;

                if (hasDestBall)
                {
                    destBall = dbInfo[destBall.Index];

                    if (destBall.IsHeld || destBall.State != PrisonDodgeballManager.DodgeballState.Neutral || !destBall.Reachable)
                    {
                        hasDestBall = false;
                    }

                }

                if (!hasDestBall)
                {
                    if (FindClosestAvailableDodgeball(out destBall))
                    {
                        hasDestBall = true;

                    }
                }

                if (hasDestBall)
                {
                    // The ball might be moving, so keep updating. GoTo() is smart enough
                    // to not keep performing full A* if it doesn't need to, so safe to call often.
                    Minion.GoTo(destBall.NavMeshPos);
                }
                else
                {
                    // No ball, so focus on defense
                    ret = DefenseDemoTransition;
                }

                return ret;
            }
        }


        // This state gets the minion close to the enemy for a throw (or a rescue of a buddy)
        class GoToThrowSpotState : MinionState
        {
            public override string Name => GoToThrowSpotStateName;

            DeferredStateTransition<MinionFSMData> CollectBallTransition;
            DeferredStateTransition<MinionFSMData, MinionScript> RescueTransition;
            DeferredStateTransition<MinionFSMData> ThrowBallTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
                RescueTransition = ParentFSM.CreateStateTransition<MinionScript>(RescueStateName, null, true);
                ThrowBallTransition = ParentFSM.CreateStateTransition(ThrowBallStateName);
            }

            public override void Enter()
            {
                base.Enter();

                Minion.GoTo(Mgr.TeamAdvance(Team).position);
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                // just in case something bad happened
                if (!Minion.HasBall)
                {
                    return CollectBallTransition;
                }

                if (Minion.ReachedTarget()) {
                    int enemyTotal = 0, friendTotal = 0;

                    PrisonDodgeballManager.OpponentInfo[] enemies = new PrisonDodgeballManager.OpponentInfo[Mgr.TeamSize];
                    Mgr.GetAllOpponentInfo(Team, ref enemies);

                    foreach (var enemy in enemies)
                        if (enemy.IsPrisoner && !enemy.IsFreedPrisoner)
                            enemyTotal++;

                    foreach (var friend in TeamData.TeamMates)
                        if (friend.IsPrisoner && !friend.IsFreedPrisoner)
                            friendTotal++;
                    
                    if (friendTotal > enemyTotal){
                        if (FindRescuableTeammate(out var x)) {
                            RescueTransition.Arg0 = x;

                            if (x.Velocity.magnitude == 0f)
                                ret = RescueTransition;
                        }
                    } else {
                        ret = ThrowBallTransition;
                    }
                }

                return ret;
            }
        }


        // Rescue a buddy
        class RescueState : MinionState<MinionScript>
        {
            public override string Name => RescueStateName;

            MinionScript buddy;

            DeferredStateTransition<MinionFSMData> CollectBallTransition;
            DeferredStateTransition<MinionFSMData> ThrowBallTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
                ThrowBallTransition = ParentFSM.CreateStateTransition(ThrowBallStateName);
            }

            public override void Enter(MinionScript m)
            {
                base.Enter(m);

                buddy = m;

                Minion.FaceTowards(buddy.transform.position);

            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                // just in case something bad happened
                if (!Minion.HasBall)
                {
                    return CollectBallTransition;
                }

                if (buddy == null || !buddy.CanBeRescued)
                {

                    if (!FindRescuableTeammate(out buddy))
                    {
                        buddy = null;
                    }

                }

                // Nothing to do without buddy in prison...
                if (buddy == null)
                    return ThrowBallTransition; // we should have a ball still...


                var canThrow = ThrowMethods.PredictThrow(Minion.HeldBallPosition, Minion.ThrowSpeed, Physics.gravity, buddy.transform.position,
                        buddy.Velocity, buddy.transform.forward, MaxAllowedThrowPositionError,
                        out var univVDir, out var speedScalar, out var interceptT, out var altT);


                var intercept = Minion.HeldBallPosition + univVDir * speedScalar * interceptT;
                Minion.FaceTowardsForThrow(intercept);

                if (
                    Minion.MaxAllowedOffAngleThrow >= Minion.AbsAngleWith(intercept) &&
                    Minion.ThrowBall(
                        univVDir, 
                        speedScalar / Minion.ThrowSpeed
                    )
                )
                    ret = CollectBallTransition;

                return ret;
            }
        }


        // Throw the ball at the enemy
        class ThrowBallState : MinionState
        {
            public override string Name => ThrowBallStateName;

            int opponentIndex = -1;
            PrisonDodgeballManager.OpponentInfo opponentInfo;
            bool hasOpponent = false;

            DeferredStateTransition<MinionFSMData> CollectBallTransition;
            DeferredStateTransition<MinionFSMData> DefenseDemoTransition;
            DeferredStateTransition<MinionFSMData, MinionScript> RescueTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
                DefenseDemoTransition = ParentFSM.CreateStateTransition(DefensiveDemoStateName);
                RescueTransition = ParentFSM.CreateStateTransition<MinionScript>(
                    RescueStateName, 
                    null, 
                    true
                );
            }


            public override void Enter()
            {
                base.Enter();


                if (Mgr.FindClosestNonPrisonerOpponentIndex(Minion.transform.position, Team, out opponentIndex))
                {
                    if (hasOpponent = Mgr.GetOpponentInfo(Team, opponentIndex, out opponentInfo))
                    {
                        Minion.FaceTowards(opponentInfo.Pos);
                    }
                }
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                // just in case something bad happened
                if (!Minion.HasBall)
                {
                    return CollectBallTransition;
                }

                // Check if opponent still valid
                if (
                    !(hasOpponent = Mgr.GetOpponentInfo(Team, opponentIndex, out opponentInfo)) ||
                    opponentInfo.IsPrisoner || opponentInfo.IsFreedPrisoner)
                {

                    if (Mgr.FindClosestNonPrisonerOpponentIndex(Minion.transform.position, Team, out opponentIndex))
                    {
                        hasOpponent = Mgr.GetOpponentInfo(Team, opponentIndex, out opponentInfo);
                    }
                }

                // Nothing to do without opponent...
                if (!hasOpponent)
                    return DefenseDemoTransition;

                int enemyTotal = 0, friendTotal = 0;

                PrisonDodgeballManager.OpponentInfo[] enemies = new PrisonDodgeballManager.OpponentInfo[Mgr.TeamSize];
                Mgr.GetAllOpponentInfo(Team, ref enemies);

                foreach (var enemy in enemies)
                    if (enemy.IsPrisoner && !enemy.IsFreedPrisoner)
                        enemyTotal++;

                foreach (var friend in TeamData.TeamMates)
                    if (friend.IsPrisoner && !friend.IsFreedPrisoner)
                        friendTotal++;
                
                if (friendTotal > enemyTotal) {
                    if (FindRescuableTeammate(out var x)) {
                        RescueTransition.Arg0 = x;

                        if (x.Velocity.magnitude == 0f)
                            ret = RescueTransition;
                    }
                }

                int navmask = NavMesh.AllAreas;


                if (!Mgr.ThrowTestEnabled || Mgr.ThrowTestRestrictTargetToSideEnabled)
                {
                    int oppTeamNavMask = 0;

                    if (Minion.Team == PrisonDodgeballManager.Team.TeamA)
                        oppTeamNavMask = Mgr.TeamBNavMeshAreaIndex;
                    else
                        oppTeamNavMask = Mgr.TeamANavMeshAreaIndex;

                    navmask = (1 << Mgr.NeutralNavMeshAreaIndex) | (1 << oppTeamNavMask) |
                                    (1 << Mgr.WalkableNavMeshAreaIndex);

                }
                
                var canThrow = ThrowMethods.PredictThrow(
                    Minion.HeldBallPosition, 
                    Minion.ThrowSpeed, 
                    Physics.gravity,
                    opponentInfo.Pos, 
                    opponentInfo.Vel, 
                    opponentInfo.Forward,
                    MaxAllowedThrowPositionError,
                    out var drivy, 
                    out var scaly, 
                    out var inty, 
                    out var temp
                );
                
                if (NavMesh.Raycast(
                    opponentInfo.Pos, 
                    drivy, 
                    out var hit, 
                    navmask
                )) {
                    if (canThrow) {
                        Minion.FaceTowardsForThrow(Minion.HeldBallPosition + drivy * scaly * inty);
                        if (Minion.ThrowBall(drivy, scaly / Minion.ThrowSpeed))
                            ret = CollectBallTransition;
                    }
                }

                return ret;
            }
        }


        // A not very effective defensive strategy. Mainly a demonstration of calling
        // Minion.Evade()
        class DefensiveDemoState : MinionState
        {
            public override string Name => DefensiveDemoStateName;

            float lastEvade;
            float evadeWaitTimeSec;
            bool doPause = false;
            float pauseStart;
            float pauseDuration;

            DeferredStateTransition<MinionFSMData> GoToThrowSpotTransition;
            DeferredStateTransition<MinionFSMData> CollectBallTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                GoToThrowSpotTransition = ParentFSM.CreateStateTransition(GoToThrowSpotStateName);
                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
            }


            protected bool RandomGoTo()
            {
                var r = Minion.GoTo(Mgr.TeamHome(Team).position + 6f * (new Vector3(Random.value, 0f, Random.value)));

                if (!r)
                {
                    Debug.LogWarning("Could not GOTO in DefenseDemoState");
                }

                return r;
            }



            public override void Enter()
            {
                base.Enter();

                RandomGoTo();

                lastEvade = Time.timeSinceLevelLoad;

                evadeWaitTimeSec = 2f * Minion.EvadeCoolDownTimeSec + 0.1f;
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                if (Minion.HasBall)
                    return GoToThrowSpotTransition;

                PrisonDodgeballManager.DodgeballInfo ball;

                if (FindClosestAvailableDodgeball(out ball))
                {
                    return CollectBallTransition;
                }

                if (!doPause && Minion.ReachedTarget())
                {
                    pauseStart = Time.timeSinceLevelLoad;
                    doPause = true;
                    pauseDuration = Random.value * 3f;
                }

                if (doPause)
                {
                    Minion.FaceTowards(Mgr.TeamPrison(Team).position);

                    if (Time.timeSinceLevelLoad - pauseStart >= pauseDuration)
                    {
                        doPause = false;
                        RandomGoTo();
                    }
                }
                else if (Time.timeSinceLevelLoad - lastEvade >= evadeWaitTimeSec)
                {

                    lastEvade = Time.timeSinceLevelLoad;

                    var r = Random.Range(0, 3);

                    MinionScript.EvasionDirection ev;

                    switch (r)
                    {
                        case 0:
                            ev = MinionScript.EvasionDirection.Brake;
                            break;
                        case 1:
                            ev = MinionScript.EvasionDirection.Left;
                            break;
                        case 2:
                            ev = MinionScript.EvasionDirection.Right;
                            break;
                        default:
                            ev = MinionScript.EvasionDirection.Brake;
                            break;
                    }

                    Minion.Evade(ev, Random.Range(0.6f, 1.0f));
                }


                return ret;
            }
        }


        // Go directly to jail. Do not pass go. Do not collect $200 
        class GoToPrisonState : MinionState
        {
            public override string Name => GoToPrisonStateName;

            int waypointIndex = 0;

            DeferredStateTransition<MinionFSMData> LeavePrisonTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                LeavePrisonTransition = ParentFSM.CreateStateTransition(LeavePrisonStateName);
            }


            public override void Enter()
            {
                base.Enter();

                waypointIndex = 0;

                Minion.GoTo(Mgr.TeamGutterEntranceLeft(Team).position);
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                if (!Minion.IsPrisoner)
                {
                    return LeavePrisonTransition;
                    //if (Minion.HasBall)
                    //    return GoToThrowSpotBallStateName;
                    //else
                    //    return GoHomeStateName;
                }

                if (Minion.ReachedTarget())
                {
                    if (waypointIndex == 0)
                    {
                        ++waypointIndex;
                        Minion.GoTo(Mgr.TeamGutterEndLeft(Team).position);
                    }
                    else if (waypointIndex == 1)
                    {
                        ++waypointIndex;
                        Minion.GoTo(Mgr.TeamPrison(Team).position);
                    }
                    else
                    {
                        Minion.FaceTowards(Mgr.TeamHome(Team).position);
                    }
                }

                return ret;
            }
        }

        // Free! 
        class LeavePrisonState : MinionState
        {
            public override string Name => LeavePrisonStateName;

            int waypointIndex = 0;

            DeferredStateTransition<MinionFSMData> GoToThrowSpotTransition;
            DeferredStateTransition<MinionFSMData> GoHomeTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                GoToThrowSpotTransition = ParentFSM.CreateStateTransition(GoToThrowSpotStateName);
                GoHomeTransition = ParentFSM.CreateStateTransition(GoHomeStateName);
            }


            public override void Enter()
            {
                base.Enter();

                waypointIndex = 0;

                Minion.GoTo(Mgr.TeamGutterEndRight(Team).position);
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                if (Minion.ReachedTarget())
                {
                    if (waypointIndex == 0)
                    {
                        ++waypointIndex;
                        Minion.GoTo(Mgr.TeamGutterEntranceRight(Team).position);
                    }
                    else
                    {
                        if (Minion.HasBall)
                            return GoToThrowSpotTransition;
                        else
                            return GoHomeTransition;

                    }
                }

                return ret;
            }
        }


        // Going home. Maybe after a jailbreak
        class GoHomeState : MinionState
        {
            public override string Name => GoHomeStateName;

            DeferredStateTransition<MinionFSMData> CollectBallTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
            }


            public override void Enter()
            {
                base.Enter();

                if (!Minion.GoTo(Mgr.TeamHome(Team).position))
                {
                    Debug.LogWarning($"Could not find a way home! NavMesh Mask: {Minion.NavMeshMaskToString()}");
                }
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                if (Minion.ReachedTarget())
                {
                    ret = CollectBallTransition;
                }

                return ret;
            }
        }


        class RestState : MinionState
        {
            public override string Name => RestStateName;

            public override void Enter()
            {
                base.Enter();

                if (!Minion.GoTo(Mgr.TeamHome(Team).position))
                {
                    Debug.LogWarning($"Could not find a way home! NavMesh Mask: {Minion.NavMeshMaskToString()}");
                }
            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                return ret;
            }
        }


        // This is a special state that never exits. It coexists with the current state.
        // It's always evaluated first. It's only job is supposed to identify global/wildcard
        // transitions (it shouldn't do anything that modifies anything externally other than
        // return a desired transition).
        class GlobalTransitionState : MinionState
        {
            public override string Name => GlobalTransitionStateName;

            bool wasPrisioner = false;

            DeferredStateTransition<MinionFSMData> RestTransition;
            DeferredStateTransition<MinionFSMData> PrisonTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                RestTransition = ParentFSM.CreateStateTransition(RestStateName);
                PrisonTransition = ParentFSM.CreateStateTransition(GoToPrisonStateName);
            }


            public override void Enter()
            {
                base.Enter();
            }

            // The global state never exits
            //public override void Exit(bool globalTransition)
            //{
            //}

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                if (Mgr.IsGameOver && !ParentFSM.CurrentState.Name.Equals(RestStateName))
                {
                    ret = RestTransition;
                }
                else if (Minion.IsPrisoner && !wasPrisioner)
                {
                    // Just switched to prisoner! Uh oh. Gotta head to prison. :-(
                    ret = PrisonTransition;

                    wasPrisioner = true;
                }
                else if (!Minion.IsPrisoner && wasPrisioner)
                {
                    wasPrisioner = false;
                }

                return ret;
            }
        }


        private void Awake()
        {
            Minion = GetComponent<MinionScript>();

            if (Minion == null)
            {
                Debug.LogWarning("No minion script");
            }
        }


        protected void InitTeamData()
        {
            Mgr.SetTeamText(Minion.Team, StudentName);

            var o = Mgr.GetTeamDataShare(Minion.Team);

            if (o == null)
            {
                TeamData = new TeamShare(Minion.Team, Mgr.TeamSize, Mgr.TotalBalls);
                Mgr.SetTeamDataShare(Minion.Team, TeamData);
            }
            else
            {
                TeamData = o as TeamShare;

                if (TeamData == null)
                {
                    Debug.LogWarning("TeamData is null!");
                }

            }

            TeamData.AddTeamMember(Minion);
        }


        // Start is called before the first frame update
        protected void Start()
        {

            Mgr = PrisonDodgeballManager.Instance;

            InitTeamData();

            var minionFSMData = new MinionFSMData(this, Minion, Mgr, Minion.Team, TeamData);

            fsm = new FiniteStateMachine<MinionFSMData>(minionFSMData);

            // Handles global/wildcard transitions. This state is a co-state that
            // never exits. Triggered transitions only change the current state.
            // The global state should only handle initiating transitions
            fsm.SetGlobalTransitionState(new GlobalTransitionState());

            fsm.AddState(new CollectBallState(), true);
            fsm.AddState(new GoToThrowSpotState());
            fsm.AddState(new ThrowBallState());
            fsm.AddState(new DefensiveDemoState());
            fsm.AddState(new GoToPrisonState());
            fsm.AddState(new LeavePrisonState());
            fsm.AddState(new GoHomeState());
            fsm.AddState(new RescueState());
            fsm.AddState(new RestState());

            //MinionStateMachine, GameAIStudentWork, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
            //Debug.Log(this.GetType().AssemblyQualifiedName);

        }

        protected void Update()
        {
            // Don't start until all the team is ready to go
            if (TeamData == null || !TeamData.IsFullyInitialized)
                return;

            fsm.Update();

            // For debugging, could repurpose the DisplayText of the Minion.
            // To do so affecting all states, implement the FSM's Update like so:
            //Minion.DisplayText(Minion.NavMeshCurrentSurfaceToString());

        }

    }
}