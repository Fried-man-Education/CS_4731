using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using GameAI;


namespace GlassJoeAI
{

    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(MinionScript))]
    public class MinionTargetDrone : MonoBehaviour
    {
        public const string StudentName = "Target Drone";

        public const string DoNothingStateName = "DoNothing";




        // Data that each FSM state gets initialized with (passed as init param)
        FiniteStateMachine<MinionFSMData> fsm;

        public MinionScript Minion { get; private set; }

        PrisonDodgeballManager Mgr;
        public TeamShare TeamData { get; private set; }

        struct MinionFSMData
        {
            public MinionTargetDrone MinionFSM { get; private set; }
            public MinionScript Minion { get; private set; }
            public PrisonDodgeballManager Mgr { get; private set; }
            public PrisonDodgeballManager.Team Team { get; private set; }
            public TeamShare TeamData { get; private set; }

            public MinionFSMData(
                MinionTargetDrone minionFSM,
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

        // Create a base class for our states to have access to the parent MinionTargetDrone, and other info
        // This class can be modified!
        abstract class MinionStateBase
        {
            public virtual string Name => throw new System.NotImplementedException();

            protected IFiniteStateMachine<MinionFSMData> ParentFSM;
            protected MinionTargetDrone MinionFSM;
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

            // Note: You can add extra methods here that you want to be available to all states

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

        // Create a base class for our states to have access to the parent MinionTargetDrone, and other info
        abstract class MinionState : MinionStateBase, IState<MinionFSMData>
        {
            public virtual void Enter() { InternalEnter(); }
        }

        // Create a base class for our states to have access to the parent MinionTargetDrone, and other info
        abstract class MinionState<S0> : MinionStateBase, IState<MinionFSMData, S0>
        {
            public virtual void Enter(S0 s) { InternalEnter(); }
        }

        // Create a base class for our states to have access to the parent MinionTargetDrone, and other info
        abstract class MinionState<S0, S1> : MinionStateBase, IState<MinionFSMData, S0, S1>
        {
            public virtual void Enter(S0 s0, S1 s1) { InternalEnter(); }
        }

        // If you need MinionState<>s with more parameters (up to four total), you can add them following the pattern above

        // Go get a ball!
        class DoNothingState : MinionState
        {
            public override string Name => DoNothingStateName;

            public override void Init(IFiniteStateMachine<MinionFSMData> parentFSM, MinionFSMData minFSMData)
            {
                base.Init(parentFSM, minFSMData);


            }

            public override void Enter()
            {
                base.Enter();
            }

            public override void Exit(bool globalTransition)
            {

            }

            public override DeferredStateTransitionBase<MinionFSMData> Update()
            {
                // drop the ball
                if (Minion.HasBall)
                    Minion.ThrowBall(Minion.transform.forward, 0.1f);

                return null;
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


            fsm.AddState(new DoNothingState(), true);


            //MinionTargetDrone, GameAIStudentWork, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
            //Debug.Log(this.GetType().AssemblyQualifiedName);

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