using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresetConfig : MonoBehaviour
{

    public GameObject ObstaclesGroup;

    public GameObject CubeObstaclePrefab;
    public GameObject SoftStarObstaclePrefab;

    public Obstacles obstacles;


    public DiscretizedSpaceMonoBehavior DiscretizedSpace;

    public MoveBall Agent;

    public Camera sceneCamera;
    public Camera gridOverlayCamera;
    public Camera pathOverlayCamera;
    public Camera searchOverlayCamera;

    public GameObject gridOverlay;
    public GameObject pathOverlay;
    public GameObject searchOverlay;


    private GameObject PathNodeMarkersGroup;
    public GameObject WaypointPrefab;
    public GameObject CustomPrefab;

    private string PathNodeMarkersGroupName = "PathNodeMarkersGroup";


    protected List<SceneConfig> SceneConfigs = new List<SceneConfig>();

    public bool AreObstaclesVisible = true;
    public bool PrevObstaclesVisible = true;

    private void Start()
    {
        if (obstacles == null)
            Debug.LogError("No obstacles");


        if (ObstaclesGroup == null)
            Debug.LogError("No Obstacles group set");

        if (CubeObstaclePrefab == null)
            Debug.LogError("No cube set");

        if (SoftStarObstaclePrefab == null)
            Debug.LogError("No soft star prefab  set");

        if (DiscretizedSpace == null)
            Debug.LogError("No discretized space set");

        if (Agent == null)
            Debug.LogError("agent (ball) not set");

        if (WaypointPrefab == null)
            Debug.LogError("no waypoint prefab");

        if (sceneCamera == null)
            Debug.LogError("no camera");

        //if (gridOverlayCamera == null)
        //    Debug.LogError("no grid overlay camera");

        //if (gridOverlay == null)
        //    Debug.LogError("grid overlay is null");

        if (pathOverlayCamera == null)
            Debug.LogError("no path overlay camera");

        if (pathOverlay == null)
            Debug.LogError("no path overlay");

        if(searchOverlayCamera == null)
        {
            Debug.LogError("no search overlay camera");
        }

        if(searchOverlay == null)
        {
            Debug.LogError("no search overlay");
        }

        LoadConfig(0);

    }

    int currConfig = -1;

    bool LoadConfig(int pos)
    {

        if (pos >= 0 && pos < SceneConfigs.Count)
        {

            var sc = SceneConfigs[pos];

            Configure(sc.WorldSize, sc.WorldOrigin, sc.AgentPos, sc.AgentScale, sc.ObstacleConfigs, sc.PathNodes, sc.GridCellSize, sc.NumExtraPathNodes, sc.Seed);

            currConfig = pos;

            return true;
        }

        return false;
    }


    // Update is called once per frame
    void Update()
    {
        int pos = -1;

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
            pos = 0;
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
            pos = 1;
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3))
            pos = 2;
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4))
            pos = 3;
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyUp(KeyCode.Keypad5))
            pos = 4;
        if (Input.GetKeyUp(KeyCode.Alpha6) || Input.GetKeyUp(KeyCode.Keypad6))
            pos = 5;
        if (Input.GetKeyUp(KeyCode.Alpha7) || Input.GetKeyUp(KeyCode.Keypad7))
            pos = 6;
        if (Input.GetKeyUp(KeyCode.Alpha8) || Input.GetKeyUp(KeyCode.Keypad8))
            pos = 7;
        if (Input.GetKeyUp(KeyCode.Alpha9) || Input.GetKeyUp(KeyCode.Keypad9))
            pos = 8;
        if (Input.GetKeyUp(KeyCode.Alpha0) || Input.GetKeyUp(KeyCode.Keypad0))
            pos = 9;


        if (pos > -1)
        {
            Debug.Log($"Loading preset position: {pos}");

            if (!LoadConfig(pos))
                Debug.Log("Config " + pos + " doesn't exist!");
            else
            {
                //return;
                PrevObstaclesVisible = true;
            }
        }


        var grid = DiscretizedSpace as GameGrid;


        if (grid != null)
        {

            if (Input.GetKeyUp(KeyCode.V))
            {
                grid.VisualizePathNetwork = !grid.VisualizePathNetwork;
                Debug.Log($"VisualizePathNetwork is now set to: {grid.VisualizePathNetwork}");
                LoadConfig(currConfig);
            }

            if (Input.GetKeyUp(KeyCode.Period))
            {
                grid.gridConnectivity = grid.gridConnectivity == GridConnectivity.EightWay ?
                    GridConnectivity.FourWay : GridConnectivity.EightWay;
                Debug.Log($"GridConnectivity is now set to: {grid.gridConnectivity}");
                LoadConfig(currConfig);
            }


        }

        var pathNetwork = DiscretizedSpace as PathNetwork;

        if(pathNetwork != null)
        {

            if (Input.GetKeyUp(KeyCode.Period))
            {
                pathNetwork.pathNetworkMode = pathNetwork.pathNetworkMode == PathNetworkMode.Predefined ?
                    PathNetworkMode.PointsOfVisibility : PathNetworkMode.Predefined;

                Debug.Log($"Path Network mode is now set to: {pathNetwork.pathNetworkMode}");
                LoadConfig(currConfig);

                PrevObstaclesVisible = true;
            }
        }


        if(Input.GetKeyUp(KeyCode.T))
        {
            AreObstaclesVisible = !AreObstaclesVisible;

            DiscretizedSpace.HideBlockingDetails = !DiscretizedSpace.HideBlockingDetails;

            LoadConfig(currConfig);

            PrevObstaclesVisible = true;

        }

        // A bit of a hack because after LoadConfig() obstacles are always visible (not really toggle-able)
        if (!AreObstaclesVisible && PrevObstaclesVisible)
        {
            //Debug.Log("Hiding Obstacles!");
            PrevObstaclesVisible = false;
            obstacles.ToggleModelsVisible();
        }
        else if(AreObstaclesVisible && !PrevObstaclesVisible)
        {
            //Debug.Log("Resetting prev obstacles visible");
            PrevObstaclesVisible = true;
        }

    }


    void SetDiscretizedSpaceSize(float scalex, float scalez)
    {

        DiscretizedSpace.transform.localScale = new Vector3(scalex, 1f, scalez);

        //var s = DiscretizedSpace.GetComponent<UVAdjustment>();
        var s = DiscretizedSpace.GetComponentInChildren<UVAdjustment>();

        if (s != null)
        {
            s.AdjustTextureCoords();
        }
    }


    void SetDiscretizedSpacePosition(float xpos, float zpos)
    {

        DiscretizedSpace.transform.position = new Vector3(xpos, 0f, zpos);

    }



    void SetCamera(Camera cam, float y_offset) 
    {
        var pos = DiscretizedSpace.transform.position;
        pos.y += y_offset; //10f
        cam.transform.position = pos;

        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        var camAspect = cam.aspect; //w/h
        var sceneScale = DiscretizedSpace.transform.localScale;
        var sceneScale_w = sceneScale.x/2f;
        var sceneScale_h = sceneScale.z/2f;
        var sceneAspect = sceneScale_w / sceneScale_h;


        //Debug.Log($"Scene Scale: w:{sceneScale_w}, h:{sceneScale_h}");
        //Debug.Log($"Scene Aspect: {sceneAspect}");
        //Debug.Log($"Cam Aspect: {camAspect}");
        

        var camHalfWidth = sceneScale_h * camAspect;

        var buffer = 1f;

        if (camHalfWidth < sceneScale_w)
        {
            //Debug.Log("Too wide!");
            cam.orthographicSize = sceneScale_w / camAspect + buffer;
        }
        else
        {
            //Debug.Log("Too tall");
            cam.orthographicSize = sceneScale_h + buffer;
        }

    }


    void PurgeOutdatedWaypoints()
    {

        var gp = GameObject.Find(PathNodeMarkersGroupName);

        if (gp != null)
        {
            gp.name = "MARKED_FOR_DELETION";
            gp.gameObject.SetActive(false);
            Destroy(gp.gameObject);
        }

    }


    protected bool NewPathNodeIsValid(Vector2 np)
    {
        bool isValid = true;

        foreach (var ob in obstacles.getObstacles())
        {
            if (ob.IsPointInPolygon(np))
            {
                isValid = false;
                break;
            }

            if (!isValid)
                break;


            Tuple<Vector2, Vector2>[] boundary = new Tuple<Vector2, Vector2>[4];
            Vector2[] boundaryCorners = new Vector2[4];
            Vector2 wOffset = new Vector2(DiscretizedSpace.Boundary.size.x, 0f);
            Vector2 hOffset = new Vector2(0f, DiscretizedSpace.Boundary.size.z);
            boundaryCorners[0] = DiscretizedSpace.BottomLeftCornerWCS;
            boundaryCorners[1] = DiscretizedSpace.BottomLeftCornerWCS + wOffset;
            boundaryCorners[2] = DiscretizedSpace.BottomLeftCornerWCS + wOffset + hOffset;
            boundaryCorners[3] = DiscretizedSpace.BottomLeftCornerWCS + hOffset;

            for (int b = 0; b < boundaryCorners.Length; ++b)
                boundary[b] = new Tuple<Vector2, Vector2>(boundaryCorners[b], boundaryCorners[(b + 1) % boundaryCorners.Length]);


            //test boundary
            for (int b = 0; b < boundary.Length; ++b)
            {
                var dist = Utils.DistanceToLine(np, boundary[b].Item1, boundary[b].Item2);
                if (dist < Agent.Radius)
                {
                    isValid = false;
                    break;
                }
            }


            float agentRadius = Agent.Radius;

            Vector2[] obstaclePoints = ob.GetPoints();
            for (int l = 0; l < obstaclePoints.Length; l++)
            {

                var a = obstaclePoints[l];
                var b = obstaclePoints[(l + 1) % obstaclePoints.Length];

                //find distance of point to line
                float dist = Utils.DistanceToLine(np, a, b);
                if (dist <= agentRadius)
                {
                    isValid = false;
                    break;
                }
            }

            if (!isValid)
                break;

        }

        return isValid;
    }


    protected void OLD_PlacePathNodes(int totalNodes, float agentScale, int seed)
    {

        //var seed = Random.Range(0, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        //Debug.Log("SEED IS: " + seed);

  
        PathNodeMarkersGroup = Utils.FindOrCreateGameObjectByName(PathNodeMarkersGroupName);


        var origin = DiscretizedSpace.BottomLeftCornerWCS;
        var width = DiscretizedSpace.Boundary.size.x;
        var height = DiscretizedSpace.Boundary.size.z;

        bool isValid;


        Vector2 posv = Vector2.zero;


        // Try to place some corner points

        var pt = Instantiate(WaypointPrefab, PathNodeMarkersGroup.transform);
        pt.transform.localScale = Vector3.one * agentScale;

        var radMult = 2f;

        //Debug.Log("AGENT RADIUS: " + Agent.Radius);


        var cornerOffset = Agent.Radius * radMult * Vector2.one;
        posv = origin + cornerOffset;
        if (NewPathNodeIsValid(posv))
            pt.transform.position = new Vector3(posv.x, 0f, posv.y);
        else
            Destroy(pt);

        pt = Instantiate(WaypointPrefab, PathNodeMarkersGroup.transform);
        pt.transform.localScale = Vector3.one * agentScale;

        posv = origin + new Vector2(width, height) - cornerOffset;
        if (NewPathNodeIsValid(posv))
            pt.transform.position = new Vector3(posv.x, 0f, posv.y);
        else
            Destroy(pt);

        pt = Instantiate(WaypointPrefab, PathNodeMarkersGroup.transform);
        pt.transform.localScale = Vector3.one * agentScale;

        cornerOffset *= new Vector2(1f, -1f);
        posv = origin + new Vector2(width, 0f) - cornerOffset;
        if (NewPathNodeIsValid(posv))
            pt.transform.position = new Vector3(posv.x, 0f, posv.y);
        else
            Destroy(pt);

        pt = Instantiate(WaypointPrefab, PathNodeMarkersGroup.transform);
        pt.transform.localScale = Vector3.one * agentScale;

        posv = origin + new Vector2(0f, height) + cornerOffset;
        if (NewPathNodeIsValid(posv))
            pt.transform.position = new Vector3(posv.x, 0f, posv.y);
        else
            Destroy(pt);


        for (int i = 0; i < totalNodes; ++i)
        {

            pt = Instantiate(WaypointPrefab, PathNodeMarkersGroup.transform);
            pt.transform.localScale = Vector3.one * agentScale;

            int count = 0;
            const int MaxCount = 100;

            do
            {
                ++count;

                if (count > MaxCount)
                {
                    isValid = false;

                    Debug.Log("Couldn't find a valid position!");
                    break;

                }

                var x = origin.x + UnityEngine.Random.Range(0f, width);
                var y = origin.y + UnityEngine.Random.Range(0f, height);

                posv = new Vector2(x, y);

                isValid = NewPathNodeIsValid(posv);

            } while (!isValid);

            if (isValid)
                pt.transform.position = new Vector3(posv.x, 0f, posv.y);
            else
                Destroy(pt);
        }


    }


    protected List<Vector2> PlacePathNodes(int totalNodes, float agentScale, int seed)
    {
        var pathNodes = new List<Vector2>();

        //var seed = Random.Range(0, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        //Debug.Log("SEED IS: " + seed);

        var origin = DiscretizedSpace.BottomLeftCornerWCS;
        var width = DiscretizedSpace.Boundary.size.x;
        var height = DiscretizedSpace.Boundary.size.z;

        bool isValid;


        Vector2 posv = Vector2.zero;


        // Try to place some corner points

        var radMult = 2f;

        //Debug.Log("AGENT RADIUS: " + Agent.Radius);


        var cornerOffset = Agent.Radius * radMult * Vector2.one;
        posv = origin + cornerOffset;
        if (NewPathNodeIsValid(posv))
            pathNodes.Add(posv);

        posv = origin + new Vector2(width, height) - cornerOffset;
        if (NewPathNodeIsValid(posv))
            pathNodes.Add(posv);

        cornerOffset *= new Vector2(1f, -1f);
        posv = origin + new Vector2(width, 0f) - cornerOffset;
        if (NewPathNodeIsValid(posv))
            pathNodes.Add(posv);


        posv = origin + new Vector2(0f, height) + cornerOffset;
        if (NewPathNodeIsValid(posv))
            pathNodes.Add(posv);


        for (int i = 0; i < totalNodes; ++i)
        {


            int count = 0;
            const int MaxCount = 100;

            do
            {
                ++count;

                if (count > MaxCount)
                {
                    isValid = false;

                    Debug.Log("Couldn't find a valid position!");
                    break;

                }

                var x = origin.x + UnityEngine.Random.Range(0f, width);
                var y = origin.y + UnityEngine.Random.Range(0f, height);

                posv = new Vector2(x, y);

                isValid = NewPathNodeIsValid(posv);

            } while (!isValid);

            if (isValid)
                pathNodes.Add(posv);
        }


        return pathNodes;

    }



    public enum ObstacleType
    {
        Cube,
        SoftStar,
        Custom
    }


    public struct ObstacleConfig
    {
        public ObstacleType otype;
        public Vector2 pos;
        public Vector2 scale;
        public float rot;
        public Vector2[] customObstVerts;

        public ObstacleConfig(ObstacleType otype, Vector2 pos, Vector2 scale, float rot, Vector2[] customObstVerts)
        {
            this.otype = otype;
            this.pos = pos;
            this.scale = scale;
            this.rot = rot;
            this.customObstVerts = customObstVerts;
        }

        public ObstacleConfig(ObstacleType otype, Vector2 pos, Vector2 scale, float rot) :
            this(otype, pos, scale, rot, null)
        {
        }
    }


    public struct SceneConfig
    {
        public Vector2 WorldSize;
        public Vector2 WorldOrigin;
        public Vector2 AgentPos;
        public float AgentScale;
        public ObstacleConfig[] ObstacleConfigs;
        public Vector2[] PathNodes;
        public float GridCellSize;
        public int NumExtraPathNodes;
        public int Seed;

        public SceneConfig(Vector2 worldSize, Vector2 worldOrigin, Vector2 agentPos, float agentScale, ObstacleConfig[] obstacleConfigs, Vector2[] pathNodes, float gridCellSize, int numExtraPathNodes, int seed)
        {
            WorldSize = worldSize;
            WorldOrigin = worldOrigin;
            AgentPos = agentPos;
            AgentScale = agentScale;
            ObstacleConfigs = obstacleConfigs;
            PathNodes = pathNodes;
            GridCellSize = gridCellSize;
            NumExtraPathNodes = numExtraPathNodes;
            Seed = seed;
        }
    }


 
    GameObject SelectPrefab(ObstacleType ot)
    {
        switch (ot)
        {
            case ObstacleType.Cube:
                return CubeObstaclePrefab;
            case ObstacleType.SoftStar:
                return SoftStarObstaclePrefab;
            case ObstacleType.Custom:
                return CustomPrefab;
            default:
                return CubeObstaclePrefab;

        }
    }

    public void Configure(Vector2 worldSize, Vector2 worldOrigin, Vector2 agentPos, float agentScale, ObstacleConfig[] obstacleConfig, Vector2[] pathNodes, float gridCellSize, int numPathNodes, int seed)
    {
        //delete old obstacles
        obstacles.DeleteObstacles();

        PurgeOutdatedWaypoints();

        //add new obstacles
        GameObject o;
        float YPOS = 0f;
        float YSCALE = 1f;
        float XROT = 0f;
        float ZROT = 0f;

        foreach (var oc in obstacleConfig)
        {
            var prefab = SelectPrefab(oc.otype);
            o = Instantiate(prefab, new Vector3(oc.pos.x, YPOS, oc.pos.y), Quaternion.Euler(XROT, oc.rot, ZROT), ObstaclesGroup.transform);
            o.transform.localScale = new Vector3(oc.scale.x, YSCALE, oc.scale.y);
            if (oc.otype == ObstacleType.Custom)
            {
                var obst = o.GetComponent<Obstacle>();
                obst.SetPoints(oc.customObstVerts);
            }

        }    

        // set ground plane size
        SetDiscretizedSpaceSize(worldSize.x, worldSize.y);

        SetDiscretizedSpacePosition(worldOrigin.x, worldOrigin.y);

        SetCamera(sceneCamera, 10f);

        if (gridOverlayCamera != null)
        {
            SetCamera(gridOverlayCamera, 100f);

            if(gridOverlay != null)
                ConfigureOverlay(gridOverlayCamera, gridOverlay, 0.01f);
        }

        if (pathOverlayCamera != null)
        {
            SetCamera(pathOverlayCamera, 200f);

            if(pathOverlay != null)
                ConfigureOverlay(pathOverlayCamera, pathOverlay, 0.02f);

        }


        if(searchOverlayCamera != null)
        {
            SetCamera(searchOverlayCamera, 300f);

            if (searchOverlay != null)
                ConfigureOverlay(searchOverlayCamera, searchOverlay, 0.03f);
        }


        // set grid rez
        var g = DiscretizedSpace as GameGrid;
        if (g != null)
            g.CellSize = gridCellSize;

        // set agent position (also resetting state)
        Agent.CurrentPosition = agentPos;

        Agent.transform.localScale = new Vector3(agentScale, agentScale, agentScale);

        obstacles.Init();

        // set path network



        //PathNodeMarkersGroup = Utils.FindOrCreateGameObjectByName(PathNodeMarkersGroupName);


        var pn = DiscretizedSpace as PathNetwork;
        if (pn != null) {

            //foreach( var pnode in pathNodes)
            //{
            //    var onode = Instantiate(WaypointPrefab, new Vector3(pnode.x, 0f, pnode.y), Quaternion.identity, PathNodeMarkersGroup.transform);
            //    onode.transform.localScale = Vector3.one * agentScale;
            //}

            var extraPathNodes = new List<Vector2>();

            if(numPathNodes > 0)
                extraPathNodes = PlacePathNodes(numPathNodes, agentScale, seed);

            pn.ResetPathNodeMarkers();
            pn.PathNodes = new List<Vector2>(pathNodes);
            pn.PathNodes.AddRange(extraPathNodes);
        }

        DiscretizedSpace.Bake();

    }


 
    void ConfigureOverlay(Camera srcCam, GameObject overlay, float offset)
    {
        var sz = srcCam.orthographicSize;
        var aspt = srcCam.aspect;
        var pos = transform.position;
        overlay.transform.localScale = new Vector3(sz * aspt * 2f, sz * 2f, 1f);
        overlay.transform.position = pos + Vector3.up * offset;
        var rndr = overlay.GetComponent<Renderer>();

        if (rndr == null)
        {
            Debug.LogError("no renderer on gridOverlay!");
        }
        else
        {
            var oc = srcCam.GetComponent<OverlayCamera>();
            if (oc == null)
            {
                Debug.LogError("no overlay camera script");
            }
            else
            {
                //Debug.Log($"overlay_mat: {rndr.sharedMaterial.mainTexture.graphicsFormat}");

                rndr.sharedMaterial = oc.Material;
            }
        }
    }


    


}
