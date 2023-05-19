using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IBallMover
{
    void OnClicked(RaycastHit hit, bool isLeft);
}
