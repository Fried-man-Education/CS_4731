using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(AudioSource))]
public class MinionAudioCancelOnDeath : MonoBehaviour {

    AudioSource minionAudio;


    private UnityAction<Vector3, MinionScript> minionDeathEventListener;


	// Use this for initialization
	void Awake () {
		
        minionAudio = GetComponent<AudioSource>();

        minionDeathEventListener = new UnityAction<Vector3, MinionScript>(minionDeathEventHandler);

	}

    void OnEnable() {

        EventManager.StartListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);

    }

    void OnDisable() {

        EventManager.StopListening<MinionDeathEvent, Vector3, MinionScript>(minionDeathEventListener);
    }
	
	// Update is called once per frame
	void Update () {
		



	}


    void minionDeathEventHandler(Vector3 pos, MinionScript ms)
    {


        if (minionAudio != null)
        {
            minionAudio.Stop();
            Destroy(this.gameObject);
        }

    }

}
