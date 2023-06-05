using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragObject : MonoBehaviour
{
    LayerMask parentMask;

    bool didTransform;

    //Obstacle obstacle;

    IDragFinishedObserver DragFinishedObs;


    private void Awake()
    {
        DragFinishedObs = GetComponent<IDragFinishedObserver>();

        //if (DragFinishedObs == null)
        //    Debug.LogError("drag finished obs is null");


    }

    // Start is called before the first frame update
    void Start()
    {
        //parentMask = this.GetComponentInParent<MaskProp>().mask;
        parentMask = Manager.Instance.DiscretizedSpaceMask.mask;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, parentMask))
        {
            didTransform = true;
            this.transform.position = new Vector3(hit.point.x, 0, hit.point.z);
        }
    }

    private void OnMouseUp()
    {
        if(didTransform)
        {
            didTransform = false;

            if(DragFinishedObs != null)
                DragFinishedObs.OnDragFinished();

            Manager.Instance.DiscretizedSpace.Bake();
        }

    }

}
