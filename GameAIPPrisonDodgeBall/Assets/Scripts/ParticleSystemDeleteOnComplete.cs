using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemDeleteOnComplete : MonoBehaviour {


    ParticleSystem particleSys;

	// Use this for initialization
	void Awake () {
		
        particleSys = GetComponent<ParticleSystem>();

	}
	
	// Update is called once per frame
	void Update () {
		
        if (!particleSys.isPlaying)
            Destroy(this.gameObject);
	}
}
