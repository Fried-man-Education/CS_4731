using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandsReporter : MonoBehaviour
{

    void OnCollisionEnter(Collision c)
    {

        if (c.impulse.magnitude > 0.5f)
        {
            //we'll just use the first contact point for simplicity
            EventManager.TriggerEvent<PlayerLandsEvent, Vector3, float>(c.contacts[0].point, c.impulse.magnitude);
        }
            

    }
}
