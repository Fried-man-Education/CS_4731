using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PCGTerrain
{


    public bool IsRoot
    {
        get
        {
            if (transform.parent == null || transform.parent.GetComponent<PCGTerrain>() == null)
                return true;
            else
                return false;
        }
    }

    private void Awake()
    {

        terrain = GetComponent<Terrain>();

        DoTerrainUpdate = false;

    }

    private void Start()
    {
        if (terrain != null)
            UpdateTerrain(terrain);
    }


    public void ProcessChildren()
    {
        PCGChildren = new List<PCGTerrain>();

        for (int c = 0; c < transform.childCount; ++c)
        {
            var o = transform.GetChild(c);
            var pcg = o.GetComponent<PCGTerrain>();

            if (pcg != null)
            {

                PCGChildren.Add(pcg);

                pcg.ProcessChildren();
            }
        }
    }


    public void DoDeserializationFromScriptableObject()
    {
        if (ConfigSerializableObject != null && ConfigSerializableObject.Config != null
            // && SerializableConfigIndex >= 0 && SerializableConfigIndex < ConfigSerializableObject.Config.Count
            )
        {
            //Debug.Log("DeSERIALIZING");

            System.Guid _guid = this.guid;
            var found = ConfigSerializableObject.Config.FindIndex(item => item.guid == _guid);

            if (found == -1)
            {
                Debug.LogError("Couldn't find Config!");
            }
            else
            {
                //Config = ConfigSerializableObject.Config[SerializableConfigIndex].DeepCopy();
                Config = ConfigSerializableObject.Config[found].DeepCopy();
            }

            this.guid = Config.guid;
            this.name = Config.Name;

            //ProcessChildren();
        }
    }

    public void DoSerializationToScriptableObject()
    {
        this.name = Config.Name;

        if (ConfigSerializableObject != null && ConfigSerializableObject.Config != null
            //&& SerializableConfigIndex >= 0 && SerializableConfigIndex < ConfigSerializableObject.Config.Count 
            )
        {
            //Debug.Log("SERIALIZING");
            //ConfigSerializableObject.Config[SerializableConfigIndex] = Config.DeepClone();
            //ConfigSerializableObject.Config[SerializableConfigIndex] = Config.DeepCopy();

            System.Guid _guid = this.guid;
            var found = ConfigSerializableObject.Config.FindIndex(item => {
                System.Guid g = item.guid;
                //Debug.Log($"Checking guid: {g} versus {_guid}");
                var ret = g == _guid;
                //Debug.Log($"Going to return: {ret}");
                return ret;
                });

            if (found == -1)
            {

                //Debug.LogError($"Couldn't find Config with guid: " + _guid);
                ConfigSerializableObject.Config.Add(this.Config.DeepCopy());
                this.guid = Config.guid;
            }
            else
            {
                //Config = ConfigSerializableObject.Config[SerializableConfigIndex].DeepCopy();
                ConfigSerializableObject.Config[found] = Config.DeepCopy();
            }
        }
    }


    //bool SerializationInitialized = false;
    bool DoTerrainUpdate = false;


    private void OnValidate()
    {

        if (this.Config == null)
            return;

        // just editing name doesn't need to update terrain...
        if (!name.Equals(this.Config.Name))
        {
            name = this.Config.Name;
            return;
        }

        name = this.Config.Name;

        DoTerrainUpdate = true;

        //if (!SerializationInitialized)
        //{
        //    Debug.Log("onval:serializationInit");
        //    SerializationInitialized = true;

        //    DoDeserializationFromScriptableObject();
        //}
        //else
        //{
        //    Debug.Log("onVal:serialToSO");
        //    DoSerializationToScriptableObject();
        //    DoTerrainUpdate = true;
        //}


    }


    // This could eventually be implemented with caching such that incremental updates could be performed
    // But since only the root has the terrain, the whole thing needs to be rebaked
    public void NotifyNeedUpdate()
    {

        var parent = transform.parent;
        var pcgParent = parent != null ? parent.GetComponent<PCGTerrain>() : null;

        if (parent == null || pcgParent == null)
        {

            if (terrain == null)
            {
                Debug.LogError("This should be the PCG root but doesn't have terrain!");
            }
            else
            {
                //propagate back down (we are at root)
                UpdateTerrain(terrain);
            }
        }
        else
        {
            pcgParent.NotifyNeedUpdate();
        }
    }


    public void RecursiveLoad()
    {
        if (ConfigSerializableObject == null)
        {
            Debug.LogError("Need to assign ConfigSerializableObject!");
            return;
        }

        if (IsRoot)
        {
            if (this.Config == null)
                this.Config = new PCGTerrainConfig();

            this.guid = ConfigSerializableObject.Config[0].guid;
        }

        DoDeserializationFromScriptableObject();

        for (int i = 0; i < Config.PCGConfigChildren.Count; ++i)
        {
            var childGUID = Config.PCGConfigChildren[i];
            System.Guid _guid = childGUID;
            var configIndex = this.ConfigSerializableObject.Config.FindIndex(item => item.guid == _guid);

            if (configIndex != -1)
            {
                var go = new GameObject("PCG_NODE");
                go.transform.parent = this.gameObject.transform;
                var newPCG = go.AddComponent<PCGTerrain>();
                newPCG.ConfigSerializableObject = this.ConfigSerializableObject;
                var config = new PCGTerrainConfig();
                newPCG.Config = config;
                newPCG.guid = childGUID;
                //newPCG.DoDeserializationFromScriptableObject();
                newPCG.RecursiveLoad();

            }
            else
            {
                Debug.LogError("Orphan guid ref found!");
            }


        }
    }


    public void RecursiveSerialize()
    {

        if(ConfigSerializableObject == null)
        {
            Debug.LogError("Need to assign ConfigSerializableObject!");
        }

        DoSerializationToScriptableObject();

        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);

            var pcg = child.GetComponent<PCGTerrain>();

            if (pcg == null)
            {
                Debug.LogError("A child that isn't PCGTerrain was found");
                break;
            }

            pcg.RecursiveSerialize();

        }

    }


    public void RecursiveDelete(bool onlyDeleteChildren, bool deleteEntryInSerializableObject)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);

            var pcg = child.GetComponent<PCGTerrain>();

            if (pcg == null)
            {
                Debug.LogError("A child that isn't PCGTerrain was found");
                break;
            }

            pcg.RecursiveDelete(false, deleteEntryInSerializableObject);

        }

        if (onlyDeleteChildren)
            return;

        var parent = this.transform.parent;
        PCGTerrain pcgParent = null;

        if (parent != null)
        {
            pcgParent = parent.GetComponent<PCGTerrain>();
            if (pcgParent != null)
            {

                //pcgParent.Config.PCGConfigChildren.Remove(this.SerializableConfigIndex);
                pcgParent.Config.PCGConfigChildren.Remove(this.guid);
                if (deleteEntryInSerializableObject)
                    pcgParent.DoSerializationToScriptableObject();
            }
        }

        if (deleteEntryInSerializableObject)
        {
            System.Guid _guid = guid;
            var removeIndex = this.ConfigSerializableObject.Config.FindIndex(item => item.guid == _guid);

            if (removeIndex != -1)
                this.ConfigSerializableObject.Config.RemoveAt(removeIndex);
            else
                Debug.LogError("NOT FOUND");
        }

#if UNITY_EDITOR
        DestroyImmediate(this.gameObject);
#else
        Destroy(this.gameObject);
#endif



        if (pcgParent != null)
            pcgParent.NotifyNeedUpdate();
        else if (onlyDeleteChildren)
            NotifyNeedUpdate();
    }



    // Update is called once per frame
    void Update()
    {

        if (DoTerrainUpdate)
        {
            DoTerrainUpdate = false;

            if (terrain != null)
                UpdateTerrain(terrain);
            else
                NotifyNeedUpdate();
        }

    }



}
