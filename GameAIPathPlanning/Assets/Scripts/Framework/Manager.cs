using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using GameAICourse;

public class Manager : MonoBehaviour
{
    public DiscretizedSpaceMonoBehavior DiscretizedSpace;
    public MaskProp DiscretizedSpaceMask;

    public Text HUDSearchText;
    //public Text HUDStudentText;

    public PathSearchAlgorithms PathSearchAlgorithm = PathSearchAlgorithms.BreadthFirstSearch;

    public bool ShowStudentName = false;

    public Material CustomPolygonMaterial;

    //public PathSearchProviderMonoBehavior SearchProvider;
    public IPathSearchProvider PathSearchProvider
    {
        get
        {
            switch(PathSearchAlgorithm)
            {
                case PathSearchAlgorithms.GreedySimple:
                    return GreedySimplePathSearch.Instance;
                case PathSearchAlgorithms.AStar:
                    return AStarPathSearch.Instance;
                case PathSearchAlgorithms.Dijkstra:
                    return DijkstrasPathSearch.Instance;
                case PathSearchAlgorithms.GreedyBestFirst:
                    return GreedyBestFirstPathSearch.Instance;
                case PathSearchAlgorithms.BreadthFirstSearch:
                    return BreadthFirstPathSearch.Instance;
                case PathSearchAlgorithms.DepthFirstSearch:
                    return DepthFirstPathSearch.Instance;
                case PathSearchAlgorithms.RandomDepthFirstSearch:
                    return RandomDepthFirstPathSearch.Instance;
                default:
                    return GreedySimplePathSearch.Instance;
            }
        }
    }
    private static Manager manager;
    public static Manager Instance
    {
        get
        {
            if (!manager)
            {
                manager = FindObjectOfType(typeof(Manager)) as Manager;
                if (!manager)
                {
                    Debug.LogError("There needs to be one active Manager script on a GameObject in your scene.");
                }
            }
            return manager;
        }
    }
    private void Awake()
    {
        if (DiscretizedSpace == null)
            Debug.LogError("No Discretized Space");
        if (DiscretizedSpaceMask == null)
            Debug.LogError("No Mask Prop for Discretized Space");
        //if (SearchProvider == null)
        //    Debug.LogError("No Search Provider!");

        if (HUDSearchText == null)
            Debug.LogError("No Text");

        //if (HUDStudentText == null)
        //    Debug.LogError("No Student Text");
    }

    private void Start()
    {
        HUDSearchText.text = PathSearchAlgorithm.ToString();

        if(ShowStudentName)
            Utils.DisplayName("AStarImpl", AStarPathSearchImpl.StudentAuthorName);
    }


    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            var currAlg = (int)PathSearchAlgorithm;
            var enums = (PathSearchAlgorithms[])System.Enum.GetValues(typeof(PathSearchAlgorithms));
            PathSearchAlgorithm = enums[(currAlg + 1) % enums.Length];

            HUDSearchText.text = PathSearchAlgorithm.ToString();
        }

        if(Input.GetKeyUp(KeyCode.Tab))
        {

            //Debug.Log("Total Scenes: " + SceneManager.sceneCountInBuildSettings);

            var curScene = SceneManager.GetActiveScene().buildIndex;

            var nextScene = (curScene + 1) % SceneManager.sceneCountInBuildSettings;

            SceneManager.LoadScene(nextScene);           
        }


    }
}
