using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UVAdjustment : MonoBehaviour
{

    MeshFilter _meshFilter;

    public bool AdjustAtStart = true;

    public float BaseScale = 1f;


    Vector2[] OrigUVs = null;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();

        if (_meshFilter == null)
            Debug.Log("no mesh filter");

    }


    Vector3 findVaryingDims(Vector3 a, Vector3 b, Vector3 c)
    {
        //Debug.Log(a + "::" + b + "::" + c);
        Vector3 dims = new Vector3(
            Approx(a.x, b.x) && Approx(a.x, c.x) &&
            Approx(b.x, c.x) ? 0f : 1f,
            Approx(a.y, b.y) && Approx(a.y, c.y) &&
            Approx(b.y, c.y) ? 0f : 1f,
            Approx(a.z, b.z) && Approx(a.z, c.z) &&
            Approx(b.z, c.z) ? 0f : 1f);

        return dims;
    }

    void Start()
    {
        if (AdjustAtStart)
        {
            AdjustTextureCoords();
        }
    }


    const float Epsilon = 0.001f;
    bool Approx(float a, float b)
    {
        return Mathf.Abs(a - b) <= Epsilon;
    }

    //public int[] DEBUG_tris;
    //public Vector3[] DEBUG_verts;

    public void AdjustTextureCoords()
    {
        var scale = transform.localScale;

        Vector2[] uvs;

        if (_meshFilter == null)
        {
            Debug.Log("No meshFilter!");
            return;
        }      

        if (OrigUVs == null)
        {
            uvs = (Vector2[])_meshFilter.sharedMesh.uv;
            OrigUVs = uvs;
        }
        else
        {
            uvs = OrigUVs;
        }
    
        Vector2[] new_uvs = new Vector2[uvs.Length];

        int[] tris = _meshFilter.mesh.triangles;
        Vector3[] verts = _meshFilter.mesh.vertices;

        //DEBUG_tris = tris;
        //DEBUG_verts = verts;

        for(int tri = 0; tri < tris.Length/3; ++tri)
        {
            float uscale = 1f;
            float vscale = 1f;

            Vector3 varyingDims = findVaryingDims(verts[tris[tri*3 + 0]], verts[tris[tri*3 + 1]], verts[tris[tri*3 + 2]]);
            float testSum = varyingDims.x + varyingDims.y + varyingDims.z;

            if(!Approx(testSum, 2f))
            {
                Debug.LogError("Could not identify aligned coord plane for triangle: " +
                    verts[tris[tri * 3 + 0]] + "::" + verts[tris[tri * 3 + 1]] + "::" + verts[tris[tri * 3 + 2]]);
            }

            if(Approx(varyingDims.x, 0f))
            {
                uscale = scale.z;
                vscale = scale.y;
            }
            else if (Approx(varyingDims.y, 0f))
            {
                uscale = scale.x;
                vscale = scale.z;
            }
            else if (Approx(varyingDims.z, 0f))
            {
                uscale = scale.x;
                vscale = scale.y;
            }

            var uv_a = uvs[tris[tri*3 + 0]];
            var uv_b = uvs[tris[tri*3 + 1]];
            var uv_c = uvs[tris[tri*3 + 2]];

            new_uvs[tris[tri*3 + 0]] = new Vector2(uv_a.x * uscale * BaseScale, uv_a.y * vscale * BaseScale);
            new_uvs[tris[tri*3 + 1]] = new Vector2(uv_b.x * uscale * BaseScale, uv_b.y * vscale * BaseScale);
            new_uvs[tris[tri*3 + 2]] = new Vector2(uv_c.x * uscale * BaseScale, uv_c.y * vscale * BaseScale);
        }

        _meshFilter.mesh.uv = new_uvs;

    }
}