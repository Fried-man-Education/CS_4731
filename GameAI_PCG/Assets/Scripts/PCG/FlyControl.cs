using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyControl : MonoBehaviour
{

    public float speed = 150f;


 

    void Update()
    {
        var dt = Time.deltaTime;
        var forward = -Input.GetAxis("Vertical") * speed * dt;
        var left = -Input.GetAxis("Horizontal") * speed * dt;

        float boost = 0f;

        if(Input.GetButton("Fire1"))
        {
            boost = speed * dt;
        }
        if(Input.GetButton("Fire2"))
        {
            boost = -speed * dt;
        }

        transform.Translate(left, boost, forward);
    }
}
