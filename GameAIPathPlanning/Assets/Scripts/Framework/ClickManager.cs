using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    //the game objects to be tested
    GameObject[] objs;
    public GameObject sphere;
    //the masks for these objects
    LayerMask[] masks;
    LayerMask maskFromBall;
    IBallMover ballMover;
    int currHit;
    bool isHit = false;
    RaycastHit hitInfo;

    Obstacles obstacles;

    public GameObject WaypointsGroup;

    bool IsMouseOver { get; set; }

    public void Awake()
    {
        obstacles = GetComponent<Obstacles>();

        if (obstacles == null)
            Debug.LogError("No obstacles");


    }

    public void Start()
    {
        currHit = -1;
        
        ballMover = sphere.GetComponent<IBallMover>();

        if (ballMover == null)
            Debug.LogError("No ball mover");

        maskFromBall = sphere.GetComponent<MaskProp>().mask;

        ProcessClickableObjects();

    }


    void ProcessClickableObjects()
    {
        //TODO I think dupe masks are not appropriate to store

        //all the objects associated with obstacles
        var obst = obstacles.getObstacles();


        int wptsCnt = 0;

        if (WaypointsGroup != null)

        {
            wptsCnt = WaypointsGroup.transform.childCount;
        }

        objs = new GameObject[obst.Count + wptsCnt];

        int i = 0;
        foreach (Obstacle p in obst)
        {
            objs[i] = p.gameObject;
            ++i;
        }

        int j = 0;

        for(j=0; j<wptsCnt; ++j)
        {
            objs[i] = WaypointsGroup.transform.GetChild(j).gameObject;
            ++i;
        }

        masks = new LayerMask[objs.Length];

        i = 0;
        foreach (GameObject obj in objs)
        {
            if(obj.GetComponent<MaskProp>())
                masks[i++] = obj.GetComponent<MaskProp>().mask;
            else
            {
                Debug.Log("ClickManager can't find maskProp on GameObject: " + obj.name);
            }
            //print("Loaded obj mask: " + masks[i]);
        }

    }


    private void LeftMouseButtonDown()
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        for (int i = 0; i < objs.Length; i++)
        {
            if (Physics.Raycast(ray, out hit, 100f, masks[i]))
            {
                currHit = i;
                //print(i + " was hit");
                isHit = true;
                hitInfo = hit;
            }
        }
        if (!isHit & Physics.Raycast(ray, out hit, 100f, maskFromBall))
        {
            ballMover.OnClicked(hit, true);
        }

    }


    private void RightMouseButtonDown()
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        for (int i = 0; i < objs.Length; i++)
        {
            if (Physics.Raycast(ray, out hit, 100f, masks[i]))
            {
                currHit = i;
                //print(i + " was hit");
                isHit = true;
                hitInfo = hit;
            }
        }
        if (!isHit & Physics.Raycast(ray, out hit, 100f, maskFromBall))
        {
            ballMover.OnClicked(hit, false);
        }

    }



    private void OnMouseUp()
    {
        isHit = false;

    }

    private void OnMouseEnter()
    {
        IsMouseOver = true;
    }

    private void OnMouseExit()
    {

        IsMouseOver = false;
    }


    private void Update()
    {

        if(IsMouseOver)
        {
            if(Input.GetMouseButtonDown(0))
            {
                LeftMouseButtonDown();
            }
            else if(Input.GetMouseButtonDown(1))
            {
                RightMouseButtonDown();
            }

        }
        
    }
}
