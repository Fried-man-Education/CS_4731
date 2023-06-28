using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GameAIStudent
{

    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(MinionScript))]
    public class MinionThrowTester : MonoBehaviour
    {
        public const string StudentName = "Bob the Minion";

        public const string GlobalTransitionStateName = "GlobalTransition";
        public const string CollectBallStateName = "CollectBall";
        public const string GoToThrowSpotStateName = "GoToThrowBall";
        public const string GoToRescueSpotStateName = "GoToRescueSpot";
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
            public MinionThrowTester MinionFSM { get; private set; }
            public MinionScript Minion { get; private set; }
            public PrisonDodgeballManager Mgr { get; private set; }
            public PrisonDodgeballManager.Team Team { get; private set; }
            public TeamShare TeamData { get; private set; }

            public MinionFSMData(
                MinionThrowTester minionFSM,
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
        // Tracking teammates is added to get you started
        public class TeamShare
        {
            public MinionScript[] TeamMates { get; private set; }
            public int TeamSize { get; private set; }
            int currTeamMateRegSpot = 0;

            public TeamShare(int teamSize)
            {
                TeamSize = teamSize;
                TeamMates = new MinionScript[TeamSize];
            }

            public void AddTeamMember(MinionScript m)
            {
                TeamMates[currTeamMateRegSpot] = m;
                ++currTeamMateRegSpot;
            }

            public bool TeamMemberCanBeRescued(out MinionScript firstHelplessMinion)
            {
                firstHelplessMinion = null;

                foreach (var m in TeamMates)
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
        }

        // Create a base class for our states to have access to the parent MinionStateMachine, and other info
        abstract class MinionStateBase
        {
            public virtual string Name => throw new System.NotImplementedException();

            protected IFiniteStateMachine<MinionFSMData> ParentFSM;
            protected MinionThrowTester MinionFSM;
            protected MinionScript Minion;
            protected PrisonDodgeballManager Mgr;
            protected PrisonDodgeballManager.Team Team;
            protected TeamShare TeamData;
            protected PrisonDodgeballManager.DodgeballInfo[] dbInfo;

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

            // determineRegion is an expensive operation to determine whether the minion
            // can go to the dodgeball. Don't ask for it if you don't need it
            protected bool UpdateAllDodgeballInfo(bool determineRegion)
            {
                if (dbInfo == null || dbInfo.Length != Mgr.TotalBalls)
                    dbInfo = new PrisonDodgeballManager.DodgeballInfo[Mgr.TotalBalls];

                return Mgr.GetAllDodgeballInfo(Minion.Team, ref dbInfo, determineRegion);
            }

            protected bool FindClosestAvailableDodgeball(out PrisonDodgeballManager.DodgeballInfo dodgeballInfo)
            {

                var dist = float.MaxValue;
                bool found = false;

                dodgeballInfo = default;

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

                return found;
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

        // Go get a ball!
        class CollectBallState : MinionState
        {
            public override string Name => CollectBallStateName;

            bool hasDestBall = false;
            PrisonDodgeballManager.DodgeballInfo destBall;

            //DeferredStateTransition<MinionFSMData> GoToThrowSpotTransition;
            DeferredStateTransition<MinionFSMData> ThrowTransition;
            //DeferredStateTransition<MinionFSMData> DefenseDemoTransition;


            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                //GoToThrowSpotTransition = ParentFSM.CreateStateTransition(GoToThrowSpotStateName);
                //DefenseDemoTransition = ParentFSM.CreateStateTransition(DefensiveDemoStateName);
                ThrowTransition = ParentFSM.CreateStateTransition(ThrowBallStateName);
            }

            public override void Enter()
            {
                base.Enter();

                Minion.GoTo(Mgr.TeamCenter(Team).position);

            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                DeferredStateTransitionBase<MinionFSMData> ret = null;

                //Debug.Log("collect:update");

                if (!Minion.HasBall)
                {
                    //Debug.Log("I need ball!");

                    Mgr.ThrowTestRequestBall(Minion);
                }
                else
                {
                    return ThrowTransition;
                }

                Minion.GoTo(Mgr.TeamCenter(Team).position);

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
            //DeferredStateTransition<MinionFSMData> DefenseDemoTransition;
            //DeferredStateTransition<MinionFSMData> GoToThrowSpotTransition;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);

                // create deferred transitions in advanced and reuse them to avoid garbage collection hit during game
                CollectBallTransition = ParentFSM.CreateStateTransition(CollectBallStateName);
                //DefenseDemoTransition = ParentFSM.CreateStateTransition(DefensiveDemoStateName);
                //GoToThrowSpotTransition = ParentFSM.CreateStateTransition(GoToThrowSpotStateName);
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
                if ((hasOpponent = Mgr.GetOpponentInfo(Team, opponentIndex, out opponentInfo)) &&
                    !opponentInfo.IsPrisoner && !opponentInfo.IsFreedPrisoner)
                {
                    //Minion.FaceTowards(opponentInfo.Pos);
                }
                else
                {
                    if (Mgr.FindClosestNonPrisonerOpponentIndex(Minion.transform.position, Team, out opponentIndex))
                    {
                        if (hasOpponent = Mgr.GetOpponentInfo(Team, opponentIndex, out opponentInfo))
                        {
                            //Minion.FaceTowards(opponentInfo.Pos);
                        }
                        else
                        {
                            return CollectBallTransition;
                        }

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

                //Debug.DrawLine(Minion.HeldBallPosition, opponentInfo.Pos, Color.magenta);


                var selection = ShotSelection.SelectThrow(Minion, opponentInfo, navmask, MaxAllowedThrowPositionError, out var projectileDir, out var projectileSpeed, out var interceptT, out var interceptPos);

                if (selection == ShotSelection.SelectThrowReturn.DoThrow)
                {
                    var speedFactor = Mathf.Min(1f, projectileSpeed / Minion.ThrowSpeed);
                    var throwRes = Minion.ThrowBall(projectileDir, speedFactor);

                    if (throwRes)
                    {
                        Minion.FaceTowardsForThrow(interceptPos);

                        return CollectBallTransition;
                    }
                    else
                    {
                        //Debug.Log("COULDN'T THROW!");
                    }
                }

                Vector3 intercept;
                if (selection == ShotSelection.SelectThrowReturn.NoThrowTargettingFailed)
                    intercept = opponentInfo.Pos;
                else
                    intercept = interceptPos;

                Minion.FaceTowardsForThrow(intercept);


                return ret;
            }
        }


        private void Awake()
        {
            Minion = GetComponent<MinionScript>();

            if (Minion == null)
                Debug.LogError("No minion script");
        }


        protected void InitTeamData()
        {
            Mgr.SetTeamText(Minion.Team, StudentName);

            var o = Mgr.GetTeamDataShare(Minion.Team);

            if (o == null)
            {
                //Debug.Log($"Team Size: {Mgr.TeamSize}");
                TeamData = new TeamShare(Mgr.TeamSize);
                Mgr.SetTeamDataShare(Minion.Team, TeamData);
            }
            else
            {
                TeamData = o as TeamShare;

                if (TeamData == null)
                    Debug.LogError("TeamData is null!");
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


            fsm.AddState(new CollectBallState(), true);
            fsm.AddState(new ThrowBallState());



        }

        protected void Update()
        {
            fsm.Update();

            // For debugging, could repurpose the DisplayText of the Minion.
            // To do so affecting all states, implement the FSM's Update like so:
            //Minion.DisplayText(Minion.NavMeshCurrentSurfaceToString());

        }

    }
}