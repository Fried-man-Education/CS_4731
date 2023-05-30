using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeTriangle : MonoBehaviour
{

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Renderer _renderer;

    Mesh mesh;

    public void SetTriangle(Vector2 A, Vector2 B, Vector2 C)
    {
        float height = 0.01f;

        var vertices = new Vector3[3];
        vertices[0] = new Vector3(A.x, height, A.y);
        vertices[1] = new Vector3(B.x, height, B.y);
        vertices[2] = new Vector3(C.x, height, C.y);

        var uvs = new Vector2[3];
        uvs[0] = new Vector2(0f, 0f);
        uvs[1] = new Vector2(1f, 0f);
        uvs[2] = new Vector2(1f, 1f);

        var tris = new int[3];
        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        mesh.name = "Triangle";

        meshFilter.mesh = mesh;  
    }
   
    private void Awake()
    {
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        _renderer = this.GetComponent<Renderer>();
        mesh = new Mesh();
    }
    // Start is called before the first frame update
    void Start()
    {
        //_renderer.material = Manager.Instance.CustomPolygonMaterial;
        _renderer.sharedMaterial = Manager.Instance.CustomPolygonMaterial;
        
        //Debug.Log(Manager.Instance.CustomPolygonMaterial.name);
        //Debug.Log(_renderer.material);
    }


}
