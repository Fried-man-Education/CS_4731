using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DodgeBallCollectable : MonoBehaviour
{
    [SerializeField]
    DodgeBall dodgeball = null;

    private void Awake()
    {
        if (dodgeball == null)
            Debug.Log("No Dodgeball");
    }

    private void OnTriggerStay(Collider other)
    {
        dodgeball.INTERNAL_OnTriggerStayFriend(other);
    }

}
