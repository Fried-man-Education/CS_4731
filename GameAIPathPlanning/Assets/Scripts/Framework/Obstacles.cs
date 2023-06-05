using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Obstacles : MonoBehaviour
{
    //to store all the child cubes' polygons
    List<Obstacle> obstacles = new List<Obstacle>();
    List<Polygon> polygons = new List<Polygon>();
    float width, height;
    Vector2 canvas_pos;
    Vector2[] boundaryPoints;
    public GameObject ObstaclesGroup;

    private void Awake()
    {
        if(ObstaclesGroup == null)
        {
            Debug.Log("No ObstaclesGroup set");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }
    public void Init()
    {
        CollectCubesAndPolygons();
    }

    public void DeleteObstacles()
    {
        for (int i = 0; i < ObstaclesGroup.transform.childCount; ++i)
        {
            var o = ObstaclesGroup.transform.GetChild(i).gameObject;
    
            Destroy(o);
        }
        ObstaclesGroup.transform.DetachChildren();
    }
    void CollectCubesAndPolygons()
    {
        polygons = new List<Polygon>();
        obstacles = new List<Obstacle>();
        //all the polygons associated with obstacles
        Obstacle[] pg = ObstaclesGroup.GetComponentsInChildren<Obstacle>();
        foreach (Obstacle p in pg)
        {
            p.Init();
            polygons.Add(p.GetPolygon());
        }
        //add them to the list;
        obstacles.AddRange(pg);
    }
    public void drawObstacles()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            ((Obstacle)obstacles[i]).DrawPolygon();
        }
        
    }



    public void ToggleModelsVisible()
    {

        for (int i = 0; i < obstacles.Count; i++)
        {
            var o = ((Obstacle)obstacles[i]);

            var mr = o.GetComponent<MeshRenderer>();

            if(mr != null)
            {
                mr.enabled = !mr.enabled;
            }
        }

    }


    public List<Obstacle> getObstacles()
    {
        return obstacles;
    }
    public List<Polygon> GetObstaclePolygons()
    {
        return polygons;
    }

    //Get a list of all points in the obstacles
    public Vector2[] GetPoints()
    {
        List<Vector2> allPoints = new List<Vector2>();
        for (int i = 0; i < obstacles.Count; i++)
        {
            Obstacle pg = (Obstacle)obstacles[i];
            //get all the points of the polygons
            allPoints.AddRange(pg.GetPoints());
        }
        return allPoints.ToArray();
    }
    //public Vector2[,] GetLines()
    //{
    //    int totalLength = 0;
    //    for (int i = 0; i < obstacles.Count; i++)
    //    {
    //        Vector2[,] obstacleLines = ((Obstacle)obstacles[i]).GetLines();
    //        totalLength += obstacleLines.GetLength(0);
    //    }
    //    Vector2[,] lines = new Vector2[totalLength,2];
    //    int k = 0;
    //    for (int i = 0; i < obstacles.Count; i++)
    //    {
    //        Vector2[,] obstacleLines = ((Obstacle)obstacles[i]).GetLines();
    //        for ( int j = 0; j < obstacleLines.GetLength(0); j++)
    //        {
    //            lines[k, 0] = obstacleLines[j, 0];
    //            lines[k, 1] = obstacleLines[j, 1];
    //            k++;
    //        }
    //    }
    //    return lines;
    //}
}
