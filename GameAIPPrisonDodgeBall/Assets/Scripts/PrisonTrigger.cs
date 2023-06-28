using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PrisonTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        var m = other.GetComponent<MinionScript>();

        if(m != null)
        {
            m.INTERNAL_TouchingPrison = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        var m = other.GetComponent<MinionScript>();

        if (m != null)
        {
            m.INTERNAL_TouchingPrison = false;
        }

    }
}
