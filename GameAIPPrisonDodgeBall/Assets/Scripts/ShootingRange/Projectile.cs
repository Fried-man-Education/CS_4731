using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public Rigidbody rbody {get; private set;}

    public ShootingRange mgr;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();

        if(rbody == null)
        {
            Debug.LogError("no rigid body");
        }
    }

    public void AcceptHit()
    {
        //Debug.Log("Collision");

        this.gameObject.SetActive(false);

        if (mgr != null)
        {
            mgr.Recycle(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        AcceptHit();

        mgr.INTERNAL_RecieveMiss();
    }

}
