using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(PCGTerrain))]
//[CanEditMultipleObjects]
public class PCGTerrainEditor : Editor
{

    PCGTerrain currTerrain = null;


    public override void OnInspectorGUI()
    {
        //Debug.Log("OnInspectorGUI");

            //Debug.Log("Startup!");

        var obj = Selection.activeGameObject;
        //Debug.Log($"inspector obj: {obj.name}");
        PCGTerrain pcg = obj.GetComponent<PCGTerrain>();

        if (pcg != null)
        {
            if (!pcg.Equals(currTerrain))
            {
                currTerrain = pcg;
                //Debug.Log("GOT PCG");
                //pcg.DoDeserializationFromScriptableObject();
            }
        }
        else
        {
            Debug.LogError("PCGTerrain is NULL!");
        }

        DrawDefaultInspector();


        if(pcg != null && pcg.IsRoot && GUILayout.Button("Load from ScriptableObject"))
        {
            pcg.RecursiveDelete(true, false);
            pcg.RecursiveLoad();
            pcg.NotifyNeedUpdate();

        }

        if (pcg != null && pcg.IsRoot && GUILayout.Button("Save to ScriptableObject"))
        {
            pcg.RecursiveSerialize();
        }


        if (GUILayout.Button("Add Child PCG Terrain Node"))
        {
            var go = new GameObject("PCG_NODE");
            go.transform.parent = pcg.gameObject.transform;
            var newPCG = go.AddComponent<PCGTerrain>();
            newPCG.ConfigSerializableObject = pcg.ConfigSerializableObject;
            var config = new PCGTerrain.PCGTerrainConfig();
            config.Name = go.name;
            config.GenNoiseCurve.AddKey(new Keyframe(0f, 0f, -1f, 1f));
            config.GenNoiseCurve.AddKey(new Keyframe(1f, 1f, 1f, -1f));
            config.ProcessParentCurve.AddKey(new Keyframe(0f, 0f, -1f, 1f));
            config.ProcessParentCurve.AddKey(new Keyframe(1f, 1f, 1f, -1f));

            newPCG.ConfigSerializableObject.Config.Add(config);
            //newPCG.SerializableConfigIndex = newPCG.ConfigSerializableObject.Config.Count - 1;
            newPCG.guid = config.guid;
            newPCG.Config = config;
            //pcg.Config.PCGConfigChildren.Add(newPCG.SerializableConfigIndex);
            pcg.Config.PCGConfigChildren.Add(newPCG.guid);

            //Debug.Log($"Just added child and size is: {pcg.Config.PCGConfigChildren.Count}");

            pcg.DoSerializationToScriptableObject();
            newPCG.DoSerializationToScriptableObject();

            //Debug.Log($"Just added child and size is: {pcg.Config.PCGConfigChildren.Count}");

            Selection.activeGameObject = newPCG.gameObject;
        }


        if(GUILayout.Button("Delete this Node (and children)"))
        {

            Selection.activeGameObject = null;
            pcg.RecursiveDelete(false, true);

        }

        

    }



}
