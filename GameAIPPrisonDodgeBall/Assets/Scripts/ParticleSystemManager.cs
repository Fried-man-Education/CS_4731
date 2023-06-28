using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemManager : MonoBehaviour
{


    public ParticleSystem minionDeathPrefab;


    private UnityAction<Vector3, MinionScript> minionDeathEventListener;



    void Awake()
    {

        minionDeathEventListener = new UnityAction<Vector3, MinionScript>(minionDeathEventHandler);
    }


    // Use this for initialization
    void Start()
    {


        			
    }


    void OnEnable()
    {

        EventManager.StartListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);
 
    }

    void OnDisable()
    {
        EventManager.StopListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);

    }


	
    // Update is called once per frame
    void Update()
    {
    }


 

  

    void minionDeathEventHandler(Vector3 worldPos, MinionScript ms)
    {

        if (minionDeathPrefab)
        {

            ParticleSystem ps = Instantiate(minionDeathPrefab, worldPos, Quaternion.identity, ms.transform);


        }
    }

  
}
