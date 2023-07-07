using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public float positionSmoothTime = 1f;		// a public variable to adjust smoothing of camera motion
    public float rotationSmoothTime = 1f;
    public float positionMaxSpeed = 50f;        //max speed camera can move
    public float rotationMaxSpeed = 50f;
	public Transform desiredPose;			// the desired pose for the camera, specified by a transform in the game
    public Transform target;
	
    protected Vector3 currentPositionCorrectionVelocity;
    //protected Vector3 currentFacingCorrectionVelocity;
    //protected float currentFacingAngleCorrVel;
    protected Quaternion quaternionDeriv;

    protected float angle;
    
	void LateUpdate ()
	{

        if (desiredPose != null)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPose.position, ref currentPositionCorrectionVelocity, positionSmoothTime, positionMaxSpeed, Time.deltaTime);

            var targForward = desiredPose.forward;
            //var targForward = (target.position - this.transform.position).normalized;

            transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation,
                Quaternion.LookRotation(targForward, Vector3.up), ref quaternionDeriv, rotationSmoothTime);

        }
    }
}
