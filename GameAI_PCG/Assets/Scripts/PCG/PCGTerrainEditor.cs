#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(PCGTerrain))]
//[CanEditMultipleObjects]
public class PCGTerrainEditor : Editor
{

    PCGTerrain currTerrain = null;



    void SaveHelper(PCGTerrainConfigSerializableObject so)
    {
        // even though it's been updated, Unity doesn't realize it! So make dirty
        EditorUtility.SetDirty(so);

        // if the assets don't get saved, the YAML of the scriptable object wont show up!
        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();

    }

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

        GUILayout.Space(20f);
        GUILayout.Label("Loading/Saving");


        if (pcg != null && pcg.IsRoot && GUILayout.Button("Load from ScriptableObject"))
        {
            pcg.RecursiveDelete(true, false);
            pcg.RecursiveLoad();
            pcg.SetDirty();
            //pcg.NotifyNeedUpdate();

        }

        if (pcg != null && pcg.IsRoot && GUILayout.Button("Save to ScriptableObject"))
        {
            pcg.RecursiveSerialize(pcg.ConfigSerializableObject);

            SaveHelper(pcg.ConfigSerializableObject);
        }


        GUILayout.Space(20f);
        GUILayout.Label("Create");


        if (GUILayout.Button("Add Child PCG Terrain Node"))
        {
            var go = new GameObject("PCG_NODE");
            go.transform.parent = pcg.gameObject.transform;
            var newPCG = go.AddComponent<PCGTerrain>();
            pcg.PCGChildren.Add(newPCG);
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

            pcg.DoSerializationToScriptableObject(pcg.ConfigSerializableObject);
            newPCG.DoSerializationToScriptableObject(pcg.ConfigSerializableObject);

            SaveHelper(pcg.ConfigSerializableObject);

            //Debug.Log($"Just added child and size is: {pcg.Config.PCGConfigChildren.Count}");

            Selection.activeGameObject = newPCG.gameObject;
        }

        GUILayout.Space(20f);
        GUILayout.Label("DANGEROUS OPERATION");

        GUILayout.TextArea("NOTE: Deletion should NOT be used to prep for Loading a new ScriptableObject. Just use Load!");

        if (GUILayout.Button("Delete this Node (and children)"))
        {

            DeleteConfirmationModal window = ScriptableObject.CreateInstance(typeof(DeleteConfirmationModal)) as DeleteConfirmationModal;

            window.deleteAction = ()=>
            {
                Selection.activeGameObject = null;
                pcg.RecursiveDelete(false, true);
                SaveHelper(pcg.ConfigSerializableObject);
            };

            window.ShowModalUtility();


        }

        GUILayout.Space(20f);
        GUILayout.Label("Testing");

        if (pcg != null && pcg.IsRoot && GUILayout.Button("Validate"))
        {

            var ret = pcg.Validate();

            if (ret)
            {
                Debug.Log("Valid!");
            }
            else
            {
                Debug.Log("NOT Valid!");
            }
        }



    }




}


#endif