using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using UnityEngine.UI;

using UnityEngine.SceneManagement;

using UnityEngine.Events;


[DefaultExecutionOrder(5)]
public class PrisonDodgeballManager : MonoBehaviour
{

    public enum DodgeballSimulationMode
    {
        FPS_60_1X_RealTime,
        FPS_60_1X_SimTime,
    };

    public DodgeballSimulationMode dodgeballSimulationMode = DodgeballSimulationMode.FPS_60_1X_RealTime;
    
    public static bool OverrideConfiguration = false;
    public static string Override_TeamAAssemblyQualifiedName = "";
    public static string Override_TeamBAssemblyQualifiedName = "";
    public static int Override_teamSize = 3;
    public static int Override_ballsPerTeam = 3;
    public static int Override_matchLengthSec = 180;
    public static DodgeballSimulationMode Override_dodgeballSimulationMode = DodgeballSimulationMode.FPS_60_1X_RealTime;

    public static Dictionary<string, int> WinsByTeamAssembly = new Dictionary<string, int>();
    public static int TotalMatches { get; private set; }

    public static bool TeamsReversed { get; private set; }


    public enum Team
    {
        TeamA,
        TeamB
    }

    public const string ArenaLayerName = "Arena";
    public const string MinionTeamALayerName = "MinionTeamA";
    public const string MinionTeamBLayerName = "MinionTeamB";
    public const string BallTeamALayerName = "BallTeamA";
    public const string BallTeamBLayerName = "BallTeamB";
    public const string BallNeutralLayerName = "BallNeutral";
    public const string PrisonerTeamALayerName = "PrisonerTeamA";
    public const string PrisonerTeamBLayerName = "PrisonerTeamB";
    public const string NavMeshCarverLayerName = "NavmeshCarver";

    public int ArenaLayerLayerIndex { get; private set; }
    public int MinionTeamALayerIndex { get; private set; }
    public int MinionTeamBLayerIndex { get; private set; }
    public int BallTeamALayerIndex { get; private set; }
    public int BallTeamBLayerIndex { get; private set; }
    public int BallNeutralLayerIndex { get; private set; }
    public int PrisonerTeamALayerIndex { get; private set; }
    public int PrisonerTeamBLayerIndex { get; private set; }
    public int NavMeshCarverLayerIndex { get; private set; }

    public const string TeamANavMeshAreaName = "TeamA";
    public const string TeamBNavMeshAreaName = "TeamB";
    public const string NeutralNavMeshAreaName = "Neutral";
    public const string WalkableNavMeshAreaName = "Walkable";
    public const string TeamAPrisonNavMeshAreaName = "TeamAPrison";
    public const string TeamBPrisonNavMeshAreaName = "TeamBPrison";

    public int TeamANavMeshAreaIndex { get; private set; }
    public int TeamBNavMeshAreaIndex { get; private set; }
    public int NeutralNavMeshAreaIndex { get; private set; }
    public int WalkableNavMeshAreaIndex { get; private set; }
    public int TeamAPrisonNavMeshAreaIndex { get; private set; }
    public int TeamBPrisonNavMeshAreaIndex { get; private set; }

    [SerializeField]
    Text TeamAUIText = null;

    [SerializeField]
    Text TeamBUIText = null;

    [SerializeField]
    Text TeamAUIWinsText = null;

    [SerializeField]
    Text TeamBUIWinsText = null;

    [SerializeField]
    Text MatchOutputText = null;

    [SerializeField]
    Color TeamATextColor = Color.green;

    [SerializeField]
    Color TeamBTextColor = Color.blue;

    [SerializeField]
    Color NeutralTextColor = Color.white;

    [SerializeField]
    string TeamAAssemblyQualifiedName = "";

    [SerializeField]
    string TeamBAssemblyQualifiedName = "";

    [SerializeField]
    bool ThrowTest = false;

    [SerializeField]
    bool ThrowTestResetMinionPos = false;

    [SerializeField]
    bool ThrowTestRestrictTargetToSide = false;

    [SerializeField]
    float ThrowTestBallRequestInterval = 0.2f;

    [SerializeField]
    Transform TeamAGutterEntranceRight = default;
    [SerializeField]
    Transform TeamAGutterEntranceLeft = default;

    [SerializeField]
    Transform TeamAGutterEndRight = default;
    [SerializeField]
    Transform TeamAGutterEndLeft = default;

    [SerializeField]
    Transform TeamAPrison = default;

    [SerializeField]
    Transform TeamAHome = default;

    [SerializeField]
    Transform TeamAAdvance = default;

    [SerializeField]
    Transform TeamACenter = default;

    [SerializeField]
    Transform TeamBGutterEntranceRight = default;
    [SerializeField]
    Transform TeamBGutterEntranceLeft = default;

    [SerializeField]
    Transform TeamBGutterEndRight = default;
    [SerializeField]
    Transform TeamBGutterEndLeft = default;

    [SerializeField]
    Transform TeamBPrison = default;

    [SerializeField]
    Transform TeamBHome = default;

    [SerializeField]
    Transform TeamBAdvance = default;

    [SerializeField]
    Transform TeamBCenter = default;

    public bool ThrowTestEnabled
    {
        get => ThrowTest;
    }

    public bool ThrowTestRestrictTargetToSideEnabled
    {
        get => ThrowTestRestrictTargetToSide;
    }

    public Transform TeamGutterEntranceRight(Team team)
    {
        if (team == Team.TeamA)
            return TeamAGutterEntranceRight;
        else
            return TeamBGutterEntranceRight;
    }


    public Transform TeamGutterEntranceLeft(Team team)
    {
        if (team == Team.TeamA)
            return TeamAGutterEntranceLeft;
        else
            return TeamBGutterEntranceLeft;
    }

    public Transform TeamGutterEndRight(Team team)
    {
        if (team == Team.TeamA)
            return TeamAGutterEndRight;
        else
            return TeamBGutterEndRight;
    }


    public Transform TeamGutterEndLeft(Team team)
    {
        if (team == Team.TeamA)
            return TeamAGutterEndLeft;
        else
            return TeamBGutterEndLeft;
    }

    public Transform TeamPrison(Team team)
    {
        if (team == Team.TeamA)
            return TeamAPrison;
        else
            return TeamBPrison;
    }

    public Transform TeamHome(Team team)
    {
        if (team == Team.TeamA)
            return TeamAHome;
        else
            return TeamBHome;
    }

    public Transform TeamAdvance(Team team)
    {
        if (team == Team.TeamA)
            return TeamAAdvance;
        else
            return TeamBAdvance;
    }

    public Transform TeamCenter(Team team)
    {
        if (team == Team.TeamA)
            return TeamACenter;
        else
            return TeamBCenter;
    }

    public Transform TeamSpawn(Team team, int i)
    {
        if (team == Team.TeamA)
            return TeamASpawnLocations[i];
        else
            return TeamBSpawnLocations[i];
    }

    public Transform TeamBallSpawn(Team team, int i)
    {
        if (team == Team.TeamA)
            return TeamABallSpawnLocations[i];
        else
            return TeamBBallSpawnLocations[i];
    }


    public void SetTeamText(Team team, string s)
    {
        if(team == Team.TeamA)
        {
            TeamAUIText.text = s;
        }
        else
        {
            TeamBUIText.text = s;
        }
    }

    [SerializeField]
    int MatchLengthSec = 180;


    [SerializeField]
    int teamSize = 3;

    public int TeamSize { get => teamSize; private set { teamSize = value; } }

    [SerializeField]
    Transform[] teamASpawnLocations = new Transform[] { };

    public Transform[] TeamASpawnLocations { get => teamASpawnLocations; private set { teamASpawnLocations = value; } }

    [SerializeField]
    Transform[] teamBSpawnLocations = new Transform[] { };

    public Transform[] TeamBSpawnLocations { get => teamBSpawnLocations; private set { teamBSpawnLocations = value; } }


    [SerializeField]
    int ballsPerTeam = 2;

    public int BallsPerTeam { get => ballsPerTeam; }

    public int TotalBalls { get => 2 * ballsPerTeam; }

    [SerializeField]
    Transform[] teamABallSpawnLocations = new Transform[] { };

    public Transform[] TeamABallSpawnLocations { get => teamABallSpawnLocations; private set { teamABallSpawnLocations = value; } }


    [SerializeField]
    Transform[] teamBBallSpawnLocations = new Transform[] { };

    public Transform[] TeamBBallSpawnLocations { get => teamBBallSpawnLocations; private set { teamBBallSpawnLocations = value; } }



    [SerializeField]
    MinionScript MinionPrefab = null;


    [SerializeField]
    DodgeBall DodgeBallPrefab = null;


    DodgeBall[] dodgeBalls = new DodgeBall[] { };

    protected DodgeBall[] DodgeBalls { get => dodgeBalls; set { dodgeBalls = value; } }


    [SerializeField]
    int throwCount = 0;

    [SerializeField]
    int hitCount = 0;

    [SerializeField]
    int missCount = 0;

    [SerializeField]
    int totalCount = 0;

    [SerializeField]
    float hitPercentage = 0f;

    [SerializeField]
    float shotsPerMinute = 0f;

    float statsTimeStart = 0f;

    public int ThrowCount { get => throwCount; private set => throwCount = value; }
    public int HitCount { get => hitCount; private set => hitCount = value; }
    public int MissCount { get => missCount; private set => missCount = value; }
    

    public void INTERNAL_AnnounceThrow()
    {
        ++ThrowCount;

    }

    void updateStats()
    {
        totalCount = HitCount + MissCount;

        hitPercentage = hitCount / (float)totalCount;

        float elapsedMin = (Time.time - statsTimeStart)/60f;

        shotsPerMinute = totalCount / elapsedMin;
    }

    public void INTERNAL_AnnounceHit()
    {
        ++HitCount;

        updateStats();
    }

    public void INTERNAL_AnnounceMiss()
    {
        ++MissCount;

        updateStats();
    }

    public void INTERNAL_ResetStats()
    {
        ThrowCount = 0;
        HitCount = 0;
        MissCount = 0;
        totalCount = 0;
        hitPercentage = 0f;
        shotsPerMinute = 0f;
        statsTimeStart = Time.time;
    }

    bool gameOver = false;

    public bool IsGameOver { get => gameOver; }

    public bool IsTie { get; private set; }

    Team WinningTeam = Team.TeamA;

    public bool IsWinner(Team team)
    {
        if (team == Team.TeamA)
        {
            return WinningTeam == Team.TeamA;
        }
        else
            return WinningTeam == Team.TeamB;
    }



    object TeamADataShare { get; set; }
    object TeamBDataShare { get; set; }

    public object GetTeamDataShare(Team team)
    {
        if (team == Team.TeamA)
            return TeamADataShare;
        else
            return TeamBDataShare;
    }

    public void SetTeamDataShare(Team team, object o)
    {
        if (team == Team.TeamA)
            TeamADataShare = o;
        else
            TeamBDataShare = o;
    }

    private static PrisonDodgeballManager Mgr;

    public static PrisonDodgeballManager Instance
    {
        get
        {
            if (!Mgr)
            {
                Mgr = FindObjectOfType(typeof(PrisonDodgeballManager)) as PrisonDodgeballManager;

                if (!Mgr)
                {
                    Debug.LogError("There needs to be one active PrisonDodgeballManager script on a GameObject in your scene.");
                }
                else
                {
                    Mgr.Init();
                }
            }

            return Mgr;
        }
    }



    private UnityAction<Vector3, MinionScript> minionDeathEventListener;





    public bool IsInit { get; private set; }


    void Init()
    {
        if (!IsInit)
        {
            IsInit = true;

 

            // JBW QualitySettings.vSyncCount = 1;

            ArenaLayerLayerIndex = LayerMask.NameToLayer(ArenaLayerName);
            MinionTeamALayerIndex = LayerMask.NameToLayer(MinionTeamALayerName);
            MinionTeamBLayerIndex = LayerMask.NameToLayer(MinionTeamBLayerName);
            BallTeamALayerIndex = LayerMask.NameToLayer(BallTeamALayerName);
            BallTeamBLayerIndex = LayerMask.NameToLayer(BallTeamBLayerName);
            BallNeutralLayerIndex = LayerMask.NameToLayer(BallNeutralLayerName);
            PrisonerTeamALayerIndex = LayerMask.NameToLayer(PrisonerTeamALayerName);
            PrisonerTeamBLayerIndex = LayerMask.NameToLayer(PrisonerTeamBLayerName);

            NavMeshCarverLayerIndex = LayerMask.NameToLayer(NavMeshCarverLayerName);

            //Debug.Log($"{ArenaLayerLayerIndex} {MinionTeamALayerIndex} {MinionTeamBLayerIndex} {BallTeamALayerIndex} {BallTeamBLayerIndex} {BallNeutralLayerIndex} {PrisonerTeamALayerIndex} {PrisonerTeamBLayerIndex}");

            TeamANavMeshAreaIndex = NavMesh.GetAreaFromName(TeamANavMeshAreaName);
            TeamBNavMeshAreaIndex = NavMesh.GetAreaFromName(TeamBNavMeshAreaName);
            NeutralNavMeshAreaIndex = NavMesh.GetAreaFromName(NeutralNavMeshAreaName);
            WalkableNavMeshAreaIndex = NavMesh.GetAreaFromName(WalkableNavMeshAreaName);
            TeamAPrisonNavMeshAreaIndex = NavMesh.GetAreaFromName(TeamAPrisonNavMeshAreaName);
            TeamBPrisonNavMeshAreaIndex = NavMesh.GetAreaFromName(TeamBPrisonNavMeshAreaName);
        }
    }



    MinionScript[] TeamAMinions;
    MinionScript[] TeamBMinions;


    //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    TimeSpan MatchTimeSpan;
    float matchStartTime = 0f;

    private void Awake()
    {
        minionDeathEventListener = new UnityAction<Vector3, MinionScript>(minionDeathEventHandler);

    }


    void OnEnable()
    {

        EventManager.StartListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);

    }

    void OnDisable()
    {
        EventManager.StopListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);

    }

    void minionDeathEventHandler(Vector3 worldPos, MinionScript ms)
    {
        if(ThrowTest && ThrowTestResetMinionPos)
        {

            if (ms.Team == Team.TeamA)
                return; //only team b is the target drone

            var nma = ms.GetComponent<NavMeshAgent>();

            if (nma != null)
            {

                float xAbsRange = 11f;
                float zAbsRange = 25f;

                float minx = -xAbsRange;
                float maxx = xAbsRange;

                float minz = -zAbsRange;
                float maxz = zAbsRange;

                if (ThrowTestRestrictTargetToSideEnabled)
                {
                    minz = -23.5f;
                    maxz = 5f;
                }

                var randDir = new Vector3(UnityEngine.Random.Range(minx, maxx), 0f, UnityEngine.Random.Range(minz, maxz));
                
                var newPos = TeamACenter.position + randDir;

                nma.Warp(newPos);
            }

        }

    }


    protected void SetSimulationMode()
    {
        if (this.dodgeballSimulationMode == DodgeballSimulationMode.FPS_60_1X_RealTime)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
        else if (this.dodgeballSimulationMode == DodgeballSimulationMode.FPS_60_1X_SimTime)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Time.captureFramerate = 60;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
    }

    private DodgeballSimulationMode lastSimMode = DodgeballSimulationMode.FPS_60_1X_RealTime;
    private void Start()
    {
        //Debug.Log("Prison Dodgeball MGR Start");

        // Set target frame rate
        //JBW QualitySettings.vSyncCount = 0;
        //JBW Application.targetFrameRate = 60;

        SetSimulationMode();
        lastSimMode = dodgeballSimulationMode;

        Cursor.visible = false;

        Init();

        if (TeamAMinions != null)
        {
            foreach (var m in TeamAMinions)
            {
                Destroy(m.gameObject);
            }
        }

        if (TeamBMinions != null)
        {
            foreach (var m in TeamBMinions)
            {
                Destroy(m.gameObject);
            }
        }

        if(dodgeBalls != null)
        {
            foreach(var b in dodgeBalls)
            {
                Destroy(b.gameObject);
            }
        }

        gameOver = false;
        IsTie = false;

        bool reverseColor = false;

        if (OverrideConfiguration)
        {
            //Debug.Log("OVERRIDING CONFIGURATION FROM INSPECTOR!");

            TeamAAssemblyQualifiedName = Override_TeamAAssemblyQualifiedName;
            TeamBAssemblyQualifiedName = Override_TeamBAssemblyQualifiedName;
            TeamSize = Override_teamSize;
            ballsPerTeam = Override_ballsPerTeam;
            MatchLengthSec = Override_matchLengthSec;
            dodgeballSimulationMode = Override_dodgeballSimulationMode;

        }
        else
        {
            //Debug.Log("Not using override config");

            if (TeamsReversed)
            {
                var tmp = TeamAAssemblyQualifiedName;
                TeamAAssemblyQualifiedName = TeamBAssemblyQualifiedName;
                TeamBAssemblyQualifiedName = tmp;
                reverseColor = true;
                
            }

            TeamsReversed = !TeamsReversed;
        }

        if (!WinsByTeamAssembly.ContainsKey(TeamAAssemblyQualifiedName))
        {
            WinsByTeamAssembly.Add(TeamAAssemblyQualifiedName, 0);
        }


        if (!WinsByTeamAssembly.ContainsKey(TeamBAssemblyQualifiedName))
        {
            WinsByTeamAssembly.Add(TeamBAssemblyQualifiedName, 0);
        }

        TeamAUIWinsText.text = WinsByTeamAssembly[TeamAAssemblyQualifiedName].ToString();
        TeamBUIWinsText.text = WinsByTeamAssembly[TeamBAssemblyQualifiedName].ToString();


        // TODO Error check sizes 

        TeamAMinions = new MinionScript[teamSize];
        TeamBMinions = new MinionScript[teamSize];

        for(int i = 0; i < teamSize; ++i)
        {

            //Debug.Log("Spawning minions");

            var ta = TeamASpawnLocations[i];

            if(ThrowTest && teamSize == 1)
            {
                // start the thrower in the middle
                ta = TeamACenter;
            }

            var ma = Instantiate<MinionScript>(MinionPrefab, ta.position, ta.rotation);
            ma.Mgr = this;
            ma.gameObject.AddComponent(System.Type.GetType(TeamAAssemblyQualifiedName));
            ma.INTERNAL_Team = Team.TeamA;
            ma.INTERNAL_SpawnIndex = i;
            ma.INTERNAL_ReverseColor = reverseColor;
            TeamAMinions[i] = ma;

            var tb = TeamBSpawnLocations[i];
            var mb = Instantiate<MinionScript>(MinionPrefab, tb.position, tb.rotation);
            mb.Mgr = this;
            mb.gameObject.AddComponent(System.Type.GetType(TeamBAssemblyQualifiedName));
            mb.INTERNAL_Team = Team.TeamB;
            mb.INTERNAL_SpawnIndex = i;
            mb.INTERNAL_ReverseColor = reverseColor;
            TeamBMinions[i] = mb;

            if(ThrowTest)
            {
                // make target minion not get knocked around by dodgeballs
                var rb = mb.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    rb.mass = 10000f;
                }

                if (!ThrowTestRestrictTargetToSide)
                {
                    // allow the target minion everywhere
                    // also constantly checked in MinionScript.Update()
                    var nma = mb.GetComponent<NavMeshAgent>();
                    if (nma != null)
                    {
                        nma.areaMask = NavMesh.AllAreas;
                    }
                    else
                    {
                        Debug.LogError($"No navmeshagent!");
                    }
                }

            }
        }

        int extraBallMult = 1;

        if (ThrowTestEnabled)
            extraBallMult = 2;


        int numBalls = ballsPerTeam * extraBallMult * 2;


        dodgeBalls = new DodgeBall[numBalls];

        int dbIndex = 0;

        for(int i = 0; i < ballsPerTeam; ++i)
        {
            for (int j = 0; j < extraBallMult; ++j)
            {
                Vector3 vertOffs = Vector3.one * 3f * (float)j;
                var ta = TeamABallSpawnLocations[i];
                var ma = Instantiate<DodgeBall>(DodgeBallPrefab, ta.position + vertOffs, ta.rotation);
                ma.ReverseColor = reverseColor;
                ma.Index = dbIndex;
                dodgeBalls[dbIndex++] = ma;
                var tb = TeamBBallSpawnLocations[i];
                var mb = Instantiate<DodgeBall>(DodgeBallPrefab, tb.position + vertOffs, tb.rotation);
                mb.ReverseColor = reverseColor;
                mb.Index = dbIndex;
                dodgeBalls[dbIndex++] = mb;
            }
        }


        MatchTimeSpan = new TimeSpan(0, 0, MatchLengthSec);
        //stopwatch.Start();
        matchStartTime = Time.timeSinceLevelLoad;

        MatchOutputText.color = NeutralTextColor;
        MatchOutputText.text =  MatchTimeSpan.ToString();

        var teamAColor = reverseColor ? TeamBTextColor : TeamATextColor;
        var teamBColor = reverseColor ? TeamATextColor : TeamBTextColor;

        TeamAUIText.color = teamAColor;
        TeamBUIText.color = teamBColor;

        TeamAUIWinsText.color = teamAColor;
        TeamBUIWinsText.color = teamBColor;
    }



    public string NavMeshMaskToString(int mask)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (0 != (mask & (1 << NeutralNavMeshAreaIndex)))
        {
            sb.Append(NeutralNavMeshAreaName);
            sb.Append(":");
        }

        if (0 != (mask & (1 << TeamANavMeshAreaIndex)) )
        {
            sb.Append(TeamANavMeshAreaName);
            sb.Append(":");
        }

        if (0 != (mask & (1 << TeamBNavMeshAreaIndex)))
        {
            sb.Append(TeamBNavMeshAreaName);
            sb.Append(":");
        }

        if (0 != (mask & (1 << WalkableNavMeshAreaIndex)))
        {
            sb.Append(WalkableNavMeshAreaName);
            sb.Append(":");
        }

        if (0 != (mask & (1 << TeamAPrisonNavMeshAreaIndex)))
        {
            sb.Append(TeamAPrisonNavMeshAreaName);
            sb.Append(":");
        }

        if (0 != (mask & (1 << TeamBPrisonNavMeshAreaIndex)))
        {
            sb.Append(TeamBPrisonNavMeshAreaName);
            sb.Append(":");
        }


        return sb.ToString();
    }


    public bool FindClosestNonPrisonerOpponentIndex(Vector3 myPos, Team myTeam, out int foundIndex)
    {
        MinionScript[] mins;

        if (myTeam == Team.TeamA)
            mins = TeamBMinions;
        else
            mins = TeamAMinions;

        var foundDist = float.MaxValue;
        foundIndex = -1;
        for(int i =0; i<mins.Length; ++i)
        {
            var m = mins[i];

            if (m.IsPrisoner || m.IsFreedPrisoner)
                continue;

            var dist = Vector3.Distance(m.transform.position, myPos);
            if(dist < foundDist)
            {
                foundIndex = i;
                foundDist = dist;
            }
        }

        return foundIndex > -1;
    }

    public enum DodgeballState
    {
        Neutral,
        Opponent,
        Team
    }


    public struct DodgeballInfo
    {
        public int Index;
        public Vector3 Pos;
        public Vector3 NavMeshPos;
        public Vector3 Vel;
        public bool IsHeld;
        public DodgeballState State;
        public int NMMask;
        public bool Reachable;


        public DodgeballInfo(int index, Vector3 pos, Vector3 navMeshPos, Vector3 vel, bool isHeld, DodgeballState state,
            int nmMask, bool reachable)
        {
            Index = index;
            Pos = pos;
            NavMeshPos = navMeshPos;
            Vel = vel;
            IsHeld = isHeld;
            State = state;
            NMMask = nmMask;
            Reachable = reachable;
        }
    }



    // Get info about dodgeball at index in dodgeball array of len TotalBalls
    // determineRegion param must be set to true in order to obtain NMMask and Reachable
    // properties in the DodgeballInfo
    public bool GetDodgeballInfo(Team myTeam, int ballIndex, out DodgeballInfo di, 
        bool determineRegion)
    {
        var opponentBallLayer = myTeam == Team.TeamA ?
                            Mgr.BallTeamBLayerIndex : Mgr.BallTeamALayerIndex;

        var myTeamBallLayer = myTeam == Team.TeamA ?
                    Mgr.BallTeamALayerIndex : Mgr.BallTeamBLayerIndex;

        var reachableMask = (1 << NeutralNavMeshAreaIndex) | (1 << WalkableNavMeshAreaIndex);

        if(myTeam == Team.TeamA)
            reachableMask = reachableMask | (1 << TeamANavMeshAreaIndex);
        else
            reachableMask = reachableMask | (1 << TeamBNavMeshAreaIndex);

        di = new DodgeballInfo();



        if (ballIndex >= 0 && ballIndex < Mgr.DodgeBalls.Length)
        {
            var b = Mgr.DodgeBalls[ballIndex];

            var pos = new Vector3( b.transform.position.x, 0f, b.transform.position.z);
            Vector3 navMeshPos = default;
            var vel = b.Velocity;
            var isHeld = b.IsHeld;

            DodgeballState state = DodgeballState.Neutral;

            if(b.Layer == opponentBallLayer)
            {
                state = DodgeballState.Opponent;
            }
            else if(b.Layer == myTeamBallLayer)
            {
                state = DodgeballState.Team;
            }
            else if(b.Layer == Mgr.BallNeutralLayerIndex)
            {
                state = DodgeballState.Neutral;
            }

            int nmMask = 0;
            bool reachable = false;

            if(determineRegion)
            {
                if (NavMesh.SamplePosition(pos, out var hit, 2f, NavMesh.AllAreas))
                {
                    nmMask = hit.mask;

                    if((hit.mask & reachableMask) > 0)
                    {
                        reachable = true;
                    }

                    navMeshPos = hit.position;
                }

            }

            di = new DodgeballInfo(ballIndex, pos, navMeshPos, vel, isHeld, state, nmMask, reachable);

            return true;
        }
        else
            return false;
    }


    public bool GetAllDodgeballInfo(Team myTeam, ref DodgeballInfo[] dodgeballInfo, bool determineRegion)
    {
        if (dodgeballInfo == null || dodgeballInfo.Length != Mgr.dodgeBalls.Length)
            return false;

        for(int i = 0; i < Mgr.dodgeBalls.Length; ++i)
        {
            DodgeballInfo dbi;
            if(GetDodgeballInfo(myTeam, i, out dbi, determineRegion))
            {
                dodgeballInfo[i] = dbi;
            }
        }

        return true;
    }



    public struct OpponentInfo
    {
        public float Index;
        public Vector3 Pos;
        public Vector3 Vel;
        public Vector3 Forward;
        public Vector3 PrevPos;
        public Vector3 PrevVel;
        public Vector3 PrevForward;
        public bool HasBall;
        public bool IsPrisoner;
        public bool IsFreedPrisoner;

        public OpponentInfo( int index, Vector3 pos,  Vector3 vel, Vector3 forward,
            Vector3 prevPos, Vector3 prevVel, Vector3 prevForward,
            bool hasBall,  
            bool isPrisoner,  bool isFreedPrisoner)
        {
            Index = index; 
            Pos = pos;
            Vel = vel;
            Forward = forward;
            PrevPos = prevPos;
            PrevForward = prevForward;
            PrevVel = prevVel;
            HasBall = hasBall;
            IsPrisoner = isPrisoner;
            IsFreedPrisoner = isFreedPrisoner;
        }
    }

    public bool GetOpponentInfo(Team myTeam, int index, out OpponentInfo oi)
    {

        oi = new OpponentInfo();

        MinionScript[] mins;

        if (myTeam == Team.TeamA)
            mins = TeamBMinions;
        else
            mins = TeamAMinions;

        if (index >= 0 && index < mins.Length)
        {
            var m = mins[index];

            var pos = m.transform.position;
            var vel = m.Velocity;
            var forward = m.transform.forward;
            var prevPos = m.prevPosition;
            var prevVel = m.prevVelocity;
            var prevForward = m.prevForward;
            var hasBall = m.HasBall;
            var isPrisoner = m.IsPrisoner;
            var isFreedPrisoner = m.IsFreedPrisoner;
            oi = new OpponentInfo(index, pos, vel, forward, prevPos, prevVel, prevForward, hasBall, isPrisoner, isFreedPrisoner);

            return true;
        }
        else
            return false;
    }


    public bool GetAllOpponentInfo(Team myTeam, ref OpponentInfo[] oppInfo)
    {
        MinionScript[] mins;

        if (myTeam == Team.TeamA)
            mins = TeamBMinions;
        else
            mins = TeamAMinions;

        if (oppInfo == null || oppInfo.Length != mins.Length)
            return false;

        for (int i = 0; i < mins.Length; ++i)
        {
            OpponentInfo oi;
            if (GetOpponentInfo(myTeam, i, out oi))
            {
                oppInfo[i] = oi;
            }
        }

        return true;
    }


    public void INTERNAL_ThrowTestReportThrow()
    {
        if (!ThrowTest)
        {
            Debug.LogError("Attempt to call INTERNAL_ThrowTestReportThrow() during regular play!");
            return;
        }

        throwTestLastRequest = Time.time;
    }

    // make sure sim settles down before throwing begins
    private float throwTestInitPause = 1f;

    private float throwTestLastRequest = 0f;

    public void ThrowTestRequestBall(MinionScript minion)
    {

        if (!ThrowTest)
        { 
            Debug.LogError("Attempt to call ThrowTestRequestBall() during regular play!");
            return;
        }

        if (Time.time < throwTestInitPause)
            return;

        if ( (Time.time - throwTestLastRequest) < ThrowTestBallRequestInterval)
            return;

        foreach (var b in dodgeBalls)
        {
            //Debug.Log("considering ball...");

            //if (b.Layer == BallNeutralLayerIndex)
            if (!b.gameObject.activeSelf)
            {
                //Debug.Log("found a ball for minion!");

                b.gameObject.SetActive(true);

               

                var brb = b.gameObject.GetComponent<Rigidbody>();

                if (brb != null)
                {
                    brb.velocity = Vector3.zero;
                    brb.angularVelocity = Vector3.zero;
                    brb.ResetInertiaTensor();

                    brb.MovePosition(minion.HeldBallPosition);
                    minion.INTERNAL_ReceiveBall(b);
                    b.INTERNAL_SetToTeam(Mgr.MinionTeamALayerIndex);
                    throwTestLastRequest = Time.time;
                    return;

                }
            }
        }


    }





    protected float maxDistSqr = Mathf.Pow(1f, 2f);

    private void Update()
    {

        if(lastSimMode != dodgeballSimulationMode)
        {
            SetSimulationMode();
        }

        Cursor.visible = false;

        if (gameOver)
        {
            if(Input.GetKeyUp(KeyCode.Space))
            {
                //Debug.Log("Starting a new match");
                //Start();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                INTERNAL_ResetStats();
                
            }
            else if(Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }

            return;
        }


        if(ThrowTest)
        {

            // Make the ball go away until requested by thrower
            foreach (var b in dodgeBalls)
            {
                if (b.Layer == BallNeutralLayerIndex)
                {
                    b.gameObject.SetActive(false);
                }
            }

                    //// balls migrate towards the minion that throws
                    //foreach(var m in TeamAMinions)
                    //{
                    //    if(m != null)// && !m.HasBall)
                    //    {
                    //        foreach(var b in dodgeBalls)
                    //        {
                    //            if(b.Layer == BallNeutralLayerIndex)
                    //            {
                    //                var brb = b.gameObject.GetComponent<Rigidbody>();

                    //                if (brb != null)
                    //                {
                    //                    var currPos = brb.transform.position;
                    //                    var currPos2D = new Vector2(currPos.x, currPos.z);
                    //                    var tpos = m.transform.position;
                    //                    var tpos2d = new Vector2(tpos.x, tpos.z);
                    //                    var sqrDist = Vector2.SqrMagnitude(currPos2D - tpos2d);
                    //                    if (sqrDist > maxDistSqr)
                    //                    {
                    //                        brb.velocity = Vector3.zero;
                    //                        //brb.AddForce((m.transform.position - brb.position).normalized * 8f, ForceMode.VelocityChange);
                    //                        brb.MovePosition(m.transform.position + Vector3.up * 5f);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}


            return;
        }


        int teamAInPrison = 0;

        bool TeamALost = false;
        foreach(var m in TeamAMinions)
        {
            if(m != null && m.IsPrisoner)
            {
                //TeamALost = false;
                ++teamAInPrison;
                //break;
            }
        }

        int teamBInPrison = 0;

        bool TeamBLost = false;
        foreach (var m in TeamBMinions)
        {
            if (m != null && m.IsPrisoner)
            {
                //TeamBLost = false;
                ++teamBInPrison;
                //break;
            }
        }

        if (teamAInPrison >= TeamSize)
            TeamALost = true;

        if (teamBInPrison >= TeamSize)
            TeamBLost = true;

        string outputText = "";

        Color outputColor = NeutralTextColor;

        if (TeamALost && TeamBLost)
        {
            //Debug.Log("Teams tied");

            outputText = "Double knockout tie!";
            gameOver = true;
            IsTie = true;
        }
        else if (TeamALost)
        {
            //Debug.Log("Team B Won!");

            outputText = $"{TeamBUIText.text} WINS!";
            gameOver = true;
            WinningTeam = Team.TeamB;
            outputColor = TeamBUIText.color;

            if (WinsByTeamAssembly.ContainsKey(TeamBAssemblyQualifiedName))
            {
                WinsByTeamAssembly[TeamBAssemblyQualifiedName] += 1;
                TeamBUIWinsText.text = WinsByTeamAssembly[TeamBAssemblyQualifiedName].ToString();

            }
            else
            {
                Debug.LogError("Could not store TeamB win!");
            }
        }
        else if (TeamBLost)
        {
            //Debug.Log("Team A Won!");

            outputText = $"{TeamAUIText.text} WINS!";          
            gameOver = true;
            WinningTeam = Team.TeamA;
            outputColor = TeamAUIText.color;

            if (WinsByTeamAssembly.ContainsKey(TeamAAssemblyQualifiedName))
            {
                WinsByTeamAssembly[TeamAAssemblyQualifiedName] += 1;
                TeamAUIWinsText.text = WinsByTeamAssembly[TeamAAssemblyQualifiedName].ToString();
            }
            else
            {
                Debug.LogError("Could not store TeamA win!");
            }
        }
        else
        {
            //var timeRem = MatchTimeSpan - stopwatch.Elapsed;
            TimeSpan elapsed = new TimeSpan(0, 0, Mathf.RoundToInt(Time.timeSinceLevelLoad - matchStartTime));
            var timeRem = MatchTimeSpan - elapsed;

            if (timeRem.TotalSeconds <= 0f)
            {

                // discourage holding all the balls, just tie
                //if (teamAInPrison > teamBInPrison)
                //    outputText = "Team B WINS tiebreaker!";
                //else if (teamBInPrison > teamAInPrison)
                //    outputText = "Team A WINS tiebreaker!";
                //else

                outputText = "TIE!";

                gameOver = true;

                IsTie = true;
            }
            else
            {
                outputText = $"Match {TotalMatches+1}: {timeRem.ToString(@"mm\:ss")}";
            }
        }

        if(gameOver)
        {
            ++TotalMatches;
        }

        MatchOutputText.color = outputColor;
        MatchOutputText.text = $"{outputText}";

        //if(gameOver)
        //{
        //    Time.timeScale = 0f;
        //}

    }

}
