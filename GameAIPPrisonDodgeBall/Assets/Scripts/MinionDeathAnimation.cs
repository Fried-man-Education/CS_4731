using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionDeathAnimation : MonoBehaviour {


    public GameObject parentGameObject;
    public MinionScript minionScript;

    public void Awake()
    {
        minionScript = parentGameObject.GetComponent<MinionScript>();

        if (minionScript == null)
            Debug.LogError("MinionScript is null");
    }

    public void ExecuteExplosion() {


        EventManager.TriggerEvent<MinionDeathEvent, Vector3, MinionScript>(parentGameObject.transform.position + Vector3.up * 0.02f, minionScript);
    }


    public void ExecuteDestroy()
    {
        //Destroy(parentGameObject);

    }
}
