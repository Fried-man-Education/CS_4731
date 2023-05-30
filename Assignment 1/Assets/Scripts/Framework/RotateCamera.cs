using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    Vector3 hit_position = Vector3.zero;
    Vector3 current_position = Vector3.zero;
    Vector3 camera_position = Vector3.zero;
    //float z = 0.0f;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            hit_position = Input.mousePosition;
            camera_position = transform.position;

        }
        if (Input.GetMouseButton(1))
        {
            current_position = Input.mousePosition;
            LeftMouseDrag();
        }
    }

    void LeftMouseDrag()
    {
        current_position.z = hit_position.z = camera_position.y;

        Vector3 direction = (current_position) - (hit_position);
        direction = direction * -1;
        this.transform.LookAt(Vector3.zero);
        this.transform.RotateAround(Vector3.zero,new Vector3(0.0f, 1.0f, 0.0f), -direction.x/100);
        this.transform.RotateAround(Vector3.zero, new Vector3(1.0f, 0.0f, 0.0f), direction.y / 100);
     }
}
