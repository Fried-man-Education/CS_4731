using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class PCGObjectPlacer : MonoBehaviour
{

    // This class isn't used, but maybe someday...

    Terrain terrain;

    // Start is called before the first frame update
    void Awake()
    {
        terrain = GetComponent<Terrain>();

        if (terrain == null)
            Debug.LogError("No terrain!");
    }

    bool IsInit = false;

    // Update is called once per frame
    void NOT_Update()
    {

        if (!IsInit)
        {
            IsInit = false;

            terrain.terrainData.SetTreeInstances(new TreeInstance[] { }, false);

            Debug.Log($"num trees: {terrain.terrainData.treeInstanceCount}");

            for (int i = 0; i < 400; ++i)
            {


                TreeInstance tree = new TreeInstance();
                tree.prototypeIndex = 0;
                tree.heightScale = 1f;
                tree.widthScale = 1f;

                var posX = Random.Range(0f, 1f);
                var posZ = Random.Range(0f, 1f);
                var posY = terrain.terrainData.GetInterpolatedHeight(posX, posZ);


                tree.position = new Vector3(posX, posY, posZ);

                //Debug.Log($"TREE is: {tree.position}");


                terrain.AddTreeInstance(tree);
                terrain.Flush();
                //print(terrain.terrainData.treeInstances.Length); //does show trees are being added to the treeInstances array

            }
        }
    }
}
