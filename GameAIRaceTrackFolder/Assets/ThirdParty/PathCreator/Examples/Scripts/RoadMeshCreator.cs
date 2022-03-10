//#define PATH_DEBUG

using System.Collections.Generic;
using System.Linq;
using PathCreation.Utility;
using UnityEngine;

namespace PathCreation.Examples {
    public class RoadMeshCreator : PathSceneTool {
        [Header ("Road settings")]
        public float roadWidth = .4f;
        [Range (0, .5f)]
        public float thickness = .15f;
        public bool flattenSurface;
        public bool meshCollider;

        [Header ("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public float textureTiling = 1;
        public float texturePerMeters = 20f;

        public float textureOffset = 0;
        public float prevTextureOffset = 0;
        //public float prevTextureOffset2 = 0;
        public float DB_UpdatedOffset = 0;
        public float DB_texOffsetErr = 0;

#if PATH_DEBUG
        public float[] DB_segLens;
#endif

        [SerializeField, HideInInspector]
        GameObject meshHolder;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;

        //Transform meshHolder;


        // which bezier control point is used to determine texture tile offset
        int BezierSegmentReferenceControlPoint = 3;


        protected virtual void Subscribe()
        {
            if (this.pathCreator != null)
            {
                //isSubscribed = true;
                this.pathCreator.pathUpdated -= PathUpdated;
                this.pathCreator.pathUpdated += PathUpdated;
            }
        }

        bool TryFindPathCreator()
        {
            // Try find a path creator in the scene, if one is not already assigned
            if (this.pathCreator == null)
            {
                if (this.GetComponent<PathCreator>() != null)
                {
                    this.pathCreator = this.GetComponent<PathCreator>();
                }
                else if (FindObjectOfType<PathCreator>())
                {
                    this.pathCreator = FindObjectOfType<PathCreator>();
                }
            }
            return this.pathCreator != null;
        }

        private void Start()
        {
            if (TryFindPathCreator())
            {
                Subscribe();
                TriggerUpdate();
            }
        }


        protected override void PathUpdated()
        {


            //Debug.Log("roadmeshpath is being updated!");

            if (pathCreator != null)
            {
                AssignMeshComponents();
                AssignMaterials();
                meshFilter.mesh = CreateRoadMesh();
                UpdateMeshCollider(meshCollider);
            }
        }
        

        Mesh CreateRoadMesh()
        {
            //var epsilon = 0.0001f;

            Vector3[] verts = new Vector3[path.NumPoints * 8];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];


            //var items = path.cumulativeLengthAtEachVertex.Where((item, index) => path.localAnchorVertexIndex.Contains(index));

            //items = items.Zip(items.Skip(1), (x, y) => y - x);

            //var arrItems = items.ToArray();

            //DB_segLens = arrItems;

            //for(int i = 1; i < DB_segLens.Length - 1; ++i)
            //{
            //    int j = i - 1;

            //    if (j == 0)
            //        continue;

            //    if (j > arrItems.Length)
            //    {
            //        Debug.LogWarning("DB_SegLens doesn't match current seglens");
            //        continue;
            //    }

            //    if(  Mathf.Abs(DB_segLens[i] - arrItems[j]) > epsilon )
            //    {
            //        Debug.LogError($"SegLens at {i} don't match: DB_seglens {DB_segLens[i]} and {arrItems[j]}");
            //    }
            //    else
            //    {
            //        Debug.Log($"APPROX EQLS !!! SegLens at {i} match: DB_seglens {DB_segLens[i]} and {arrItems[j]}");
            //    }

            //}

            //DB_segLens = arrItems;

            //var numTextureRepeats = path.length / texturePerMeters;

            //Debug.Log($"pathLen: {path.length} numTextureRepeats: {numTextureRepeats} textOffs: {textureOffset} last ptime: {path.times[path.times.Length-1]}");

            //textureTiling = numTextureRepeats;

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
            int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            // this should be the beginning of the 2nd bezier segment in path form
            //path.localAnchorVertexIndex[1]
            //int prevSeg = 1;
            //bool secondSegFound = false;
            //float newTexOffset = 0f;

            var prevSegUpdatedOffset = 0f;

            // BezierSegmentOffset is one less due to the first segment being deleted as it falls off behind the agent
            if (path.localAnchorVertexIndex.Length > (BezierSegmentReferenceControlPoint-1))
            {
                prevSegUpdatedOffset = (  path.cumulativeLengthAtEachVertex[path.localAnchorVertexIndex[BezierSegmentReferenceControlPoint - 1]] * texturePerMeters) % 1f;
            }

            DB_UpdatedOffset = prevSegUpdatedOffset;

            // Ok, now compare to the old offet and adjust accordingly


            textureOffset = prevTextureOffset - prevSegUpdatedOffset;


            //while (textureOffset < 0f)
            //    textureOffset += 1f;

            DB_texOffsetErr = textureOffset;

            //textureOffset = (textureOffset) % 1f;

            //if (path.localAnchorVertexIndex.Length > (BezierSegmentReferenceControlPoint - 1))
            //{
            //    var testOffset = (textureOffset + path.cumulativeLengthAtEachVertex[path.localAnchorVertexIndex[BezierSegmentReferenceControlPoint - 1]] * texturePerMeters) % 1f;

            //    if (Mathf.Abs(testOffset - ((prevTextureOffset ))) > epsilon)
            //    {
            //        Debug.LogError($"testOffset doesn't match prevTextureOffset {testOffset} and {prevTextureOffset}");
            //    }
            //    else
            //    {
            //        Debug.Log("GOOD");
            //    }    
            //}


            for (int i = 0; i < path.NumPoints; i++) {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross (path.GetTangent (i), path.GetNormal (i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal (i) : Vector3.Cross (localUp, path.GetTangent (i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint (i) - localRight * Mathf.Abs (roadWidth);
                Vector3 vertSideB = path.GetPoint (i) + localRight * Mathf.Abs (roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                // ... Nope, it needs to be based on distance for a dynamically generated path
                // because the length varies over time as different bezier segments get added/deleted
                // var texv = textureOffset + path.times[i];

                var texv = textureOffset + path.cumulativeLengthAtEachVertex[i] * texturePerMeters;

                var curSeg = path.GetBezierSegmentIndex(i);

                //if(!secondSegFound && curSeg <= prevSeg)
                //{
                //    secondSegFound = true;
                //    newTexOffset = texv;
                //}

                uvs[vertIndex + 0] = new Vector2 (0, texv);
                uvs[vertIndex + 1] = new Vector2 (1, texv);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;
                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;
                // Sides of road normals
                normals[vertIndex + 4] = -localRight;
                normals[vertIndex + 5] = localRight;
                normals[vertIndex + 6] = -localRight;
                normals[vertIndex + 7] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop) {
                    for (int j = 0; j < triangleMap.Length; j++) {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++) {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                    }

                }

                vertIndex += 8;
                triIndex += 6;

            } //for


            // Need to remember the texture coord offset so that we can adjust the offset as the path changes...

            if (path.localAnchorVertexIndex.Length > BezierSegmentReferenceControlPoint)
            {
                prevTextureOffset = (textureOffset + path.cumulativeLengthAtEachVertex[path.localAnchorVertexIndex[BezierSegmentReferenceControlPoint]] * texturePerMeters) % 1f;
            }

            //if (path.localAnchorVertexIndex.Length > BezierSegmentReferenceControlPoint + 1)
            //{
            //    prevTextureOffset2 = (textureOffset + path.cumulativeLengthAtEachVertex[path.localAnchorVertexIndex[BezierSegmentReferenceControlPoint + 1]] * texturePerMeters) % 1f;
            //}

            mesh.Clear ();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.subMeshCount = 3;
            mesh.SetTriangles (roadTriangles, 0);
            mesh.SetTriangles (underRoadTriangles, 1);
            mesh.SetTriangles (sideOfRoadTriangles, 2);
            mesh.RecalculateBounds ();

            return mesh;
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        void AssignMeshComponents()
        {
            // Find/creator mesh holder object in children
            string meshHolderName = "Road Mesh Holder";
            var trans = transform.Find(meshHolderName);
            if(trans != null)
            {
                meshHolder = trans.gameObject;
            }
            if (meshHolder == null) {
                meshHolder = new GameObject (meshHolderName);
            }

            meshHolder.transform.rotation = Quaternion.identity;
            meshHolder.transform.position = Vector3.zero;
            meshHolder.transform.localScale = Vector3.one;

            // Ensure mesh renderer and filter components are assigned
            if (!meshHolder.gameObject.GetComponent<MeshFilter> ()) {
                meshHolder.gameObject.AddComponent<MeshFilter> ();
            }
            if (!meshHolder.GetComponent<MeshRenderer> ()) {
                meshHolder.gameObject.AddComponent<MeshRenderer> ();
            }

            meshRenderer = meshHolder.GetComponent<MeshRenderer> ();
            meshFilter = meshHolder.GetComponent<MeshFilter> ();
            if (mesh == null) {
                mesh = new Mesh ();
            }
            meshFilter.sharedMesh = mesh;
        }

        void AssignMaterials () {
            if (roadMaterial != null && undersideMaterial != null) {
                meshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
                meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3 (1, textureTiling);
            }
        }

        //update meshcolider if enabled
        void UpdateMeshCollider(bool meshColliderEnable)
        {
            if (meshHolder != null)
            {
                MeshCollider meshCol = meshHolder.GetComponent<MeshCollider>();
                if (meshCol != null)
                {
                    if (meshColliderEnable)
                    {
                        meshCol.sharedMesh = meshHolder.GetComponent<MeshFilter>().sharedMesh;
                    }
                    else
                    {
                        DestroyImmediate(meshCol);
                    }                
                }
                else if (meshColliderEnable)
                {
                    meshHolder.gameObject.AddComponent<MeshCollider>();

                    // TODO why can't mesh be set here?
                }
            }
        }


    }
}