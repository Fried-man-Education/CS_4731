#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PCGTerrain
{

    bool descendentDirty = true;

    bool localDirty = true;


    public void SetDirty()
    {
        localDirty = true;
    }


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


    // Awake and Start Cannot be used in editor mode, but for live game these could be useful

    //private void Awake()
    //{
    //    //Debug.Log("AWAKE");

    //    //terrain = GetComponent<Terrain>();

    //    //SetupTerrain();

    //    ////DoTerrainUpdate = false;

    //}

    //private void Start()
    //{
    //    //Debug.Log("START");

    //    //if (terrain != null)
    //    //{
    //    //    Debug.Log("terrain found in start!");

    //    //    SetupTerrain();
    //    //    //UpdateTerrain(terrain);
    //    //    RootUpdate();
    //    //}
    //}





    public void DoDeserializationFromScriptableObject()
    {
        if (ConfigSerializableObject != null && ConfigSerializableObject.Config != null
            // && SerializableConfigIndex >= 0 && SerializableConfigIndex < ConfigSerializableObject.Config.Count
            )
        {
            Debug.Log("DeSERIALIZING");

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


    void DoSerializationToScriptableObject( )
    {
        DoSerializationToScriptableObject(this.ConfigSerializableObject);
    }

    public void DoSerializationToScriptableObject(PCGTerrainConfigSerializableObject so)
    {
        //Debug.Log("DoSerializationTOScriptableObject()BEGIN");

        this.name = Config.Name;

        if (so == null || so.Config == null)
            Debug.LogError($"CANNOT SERIALIZE DUE TO NULL {so} {so.Config}");


        if (so != null && so.Config != null
            //&& SerializableConfigIndex >= 0 && SerializableConfigIndex < ConfigSerializableObject.Config.Count 
            )
        {
            // update in case it is different for some reason. Should match for adding/deleting nodes later...
            this.ConfigSerializableObject = so;


            System.Guid _guid = this.guid;
            var found = so.Config.FindIndex(item => {
                System.Guid g = item.guid;
                //Debug.Log($"Checking guid: {g} versus {_guid}");
                var ret = g == _guid;
                //Debug.Log($"Going to return: {ret}");
                return ret;
                });

            if (found == -1)
            {

                //Debug.LogError($"Couldn't find Config with guid: " + _guid);
                so.Config.Add(this.Config.DeepCopy());
                this.guid = Config.guid;
            }
            else
            {
                //Config = ConfigSerializableObject.Config[SerializableConfigIndex].DeepCopy();
                so.Config[found] = Config.DeepCopy();
            }
        }
    }


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

        localDirty = true;

    }


    public void RecursiveLoad()
    {

        //Debug.Log("Sav-aye Low-Add!");

        if (ConfigSerializableObject == null)
        {
            Debug.LogError("Need to assign ConfigSerializableObject!");
            return;
        }

        if (IsRoot)
        {
            //Debug.Log("ROOT");
            if (this.Config == null)
                this.Config = new PCGTerrainConfig();

            if (ConfigSerializableObject.Config.Count <= 0)
            {
                this.guid = new StringyGuid();

                Debug.LogWarning("The assigned ScriptableObject does not contain any nodes. You can save to add the current node");

                return;
            }
            else
                this.guid = ConfigSerializableObject.Config[0].guid;
        }

        DoDeserializationFromScriptableObject();

        PCGChildren = new List<PCGTerrain>();

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
                PCGChildren.Add(newPCG);
                newPCG.ConfigSerializableObject = this.ConfigSerializableObject;
                var config = new PCGTerrainConfig();
                newPCG.Config = config;
                newPCG.guid = childGUID;
                //newPCG.DoDeserializationFromScriptableObject();
                newPCG.RecursiveLoad();

            }
            else
            {
                Debug.LogError($"Orphan guid ref found! -> {_guid}");
            }


        }
    }


    public void RecursiveSerialize(PCGTerrainConfigSerializableObject so)
    {

        if(so == null)
        {
            Debug.LogError("Need to assign ConfigSerializableObject!");
        }

        DoSerializationToScriptableObject(so);

        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);

            var pcg = child.GetComponent<PCGTerrain>();

            if (pcg == null)
            {
                Debug.LogError("A child that isn't PCGTerrain was found");
                break;
            }

            pcg.RecursiveSerialize(so);

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

        PCGChildren = new List<PCGTerrain>();


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
                if(!pcgParent.PCGChildren.Remove(this))
                {
                    Debug.LogError("Failed to remove node from parent on delete");
                }
                else
                {
                    Debug.Log("Removed child node on delete");
                }

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


        //if (pcgParent != null)
        //    pcgParent.NotifyNeedUpdate();
        //else if (onlyDeleteChildren)
        //    NotifyNeedUpdate();


        if (pcgParent != null)
            pcgParent.SetDirty();
        else if (onlyDeleteChildren)
            localDirty = true;


    }



    // Update is called once per frame
    void Update()
    {
        RootUpdate();
    }

    public bool Validate()
    {
        var validatedNodes = new List<PCGTerrain>();
        return Validate(this.ConfigSerializableObject, ref validatedNodes);
    }

    public bool Validate(PCGTerrainConfigSerializableObject configSO, ref List<PCGTerrain> validatedNodes)
    {
        PCGTerrainConfig foundItem;

        if (ConfigSerializableObject == null)
        {
            Debug.LogError($"Need to assign ConfigSerializableObject to {name}!");
            return false;
        }

        if(ConfigSerializableObject != configSO)
        {
            Debug.LogError($"{name} has a configSO that doesn't match expected!");
            return false;
        }

        if (this.Config == null)
        {
            Debug.LogError($"Null Config on {name}");
            return false;
        }

        if (IsRoot)
        {
            //Debug.Log("ROOT");

            if(!this.guid.Equals(ConfigSerializableObject.Config[0].guid))
            {
                Debug.LogError($"Root ({name}) has GUID ({this.guid}) that does not match first element of SO ({ConfigSerializableObject.Config[0].guid})");
                return false;
            }

            foundItem = ConfigSerializableObject.Config[0];
        }
        else
        {
            System.Guid _guid = this.guid;
            var found = ConfigSerializableObject.Config.FindIndex(item => item.guid == _guid);

            if (found == -1)
            {
                Debug.LogError($"Couldn't find {name} with guid:{guid} in SO");
                return false;
            }

            foundItem = ConfigSerializableObject.Config[found];

        }

        if(!Config.Equals(foundItem))
        {
            Debug.LogError($"{name} does not match conf in SO");
            return false;
        }

        var pcgChildSet = new HashSet<PCGTerrain>(PCGChildren);

        for (int i = 0; i < transform.childCount; ++i)
        {
            var child = transform.GetChild(i);

            var pcg = child.GetComponent<PCGTerrain>();

            if (pcg == null)
            {
                Debug.LogError($"A child that isn't PCGTerrain was found under {name}");
                return false;
            }

            System.Guid _guid = pcg.guid;
            var childIndex = PCGChildren.FindIndex(item => item.guid == _guid);

            if(childIndex < 0)
            {
                Debug.LogError($"Node {name}::{guid} has hierarchy child {pcg.name}::{pcg.guid} but doesn't appear in PCGChildren");
                return false;
            }

            var pcgChild = PCGChildren[childIndex];

            if (!pcgChildSet.Remove(pcgChild))
            {
                Debug.LogError($"Failed to remove childNode {pcgChild.name} may indicate a dupe in the hierarchy!");
                return false;
            }

            pcg.Validate(configSO, ref validatedNodes);

        } //for

        if(pcgChildSet.Count > 0)
        {
            Debug.LogError($"PCGChildren were found in {name} that weren't also hierarchy children!");
            return false;
        }

        if (validatedNodes.Contains(this))
        {
            Debug.LogError($"Node {name}::{guid} appears more than once in the hierarchy!");
            return false;
        }
        else
        {
            validatedNodes.Add(this);
        }

        // TODO check for orphans in the SO
        if (IsRoot)
        {
            foreach(var c in ConfigSerializableObject.Config)
            {
                System.Guid _guid = c.guid;
                var found = validatedNodes.FindIndex(item => item.guid == _guid);

                if(found == -1)
                {
                    Debug.LogError($"ConfigSerializableObject contains orphan node: {c.Name}::{c.guid}");
                    return false;
                }
            }
        }



        return true;

    }


}


#endif