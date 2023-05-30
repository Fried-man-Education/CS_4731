using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionReporter : MonoBehaviour
{

    public Vector2 GetPosition()
    {
        return new Vector2(this.transform.position.x, this.transform.position.z);
    }

}
