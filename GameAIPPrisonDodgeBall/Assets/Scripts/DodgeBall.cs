using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class DodgeBall : MonoBehaviour
{

    PrisonDodgeballManager mgr;

    [SerializeField]
    Renderer rend = null;

    Rigidbody rbody;

    [SerializeField]
    Collider TriggerCollider = null;

    public Material TeamAMaterial;
    public Material TeamBMaterial;
    public Material NeutralMaterial;


    public int Index { get; set; }

    public Vector3 Velocity { get { return rbody.velocity; } }

    public int Layer { get { return gameObject.layer; } }


    protected bool hasGottenHit = false;

    protected float timeOfLastCollision = 0f;

    [SerializeField]
    bool reverseColor = false;

    public bool ReverseColor
    {
        get
        {
            return reverseColor;
        }
        set
        {
            reverseColor = value;
        }
    }


    private bool isHeld = false;

    public bool IsHeld
    {
        get { return isHeld; }
        set
        {
            isHeld = value;

            if (isHeld)
            {
                //get rid of dumb warning. will switch back to continuous at throw time
                rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rbody.isKinematic = true;
            }
            else
            {
                transform.parent = null;
                rbody.isKinematic = false;
                rbody.collisionDetectionMode = CollisionDetectionMode.Continuous;            
            }
        }
    }


    public void INTERNAL_SetToNeutral()
    {
        INTERNAL_SetBallType(mgr.BallNeutralLayerIndex);

        hasGottenHit = false;
    }

    public void INTERNAL_Throw(Vector3 vel)
    {
        if (IsHeld)
        {
            IsHeld = false;
            transform.parent = null; // TODO set to group, notify manager?
            rbody.isKinematic = false;
            rbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;

            rbody.AddForce(vel, ForceMode.VelocityChange);
        }
    }


    void Awake()
    {
        rbody = GetComponent<Rigidbody>();

        if (rbody == null)
            Debug.Log("no rigidbody");

        if (rend == null)
            Debug.LogError("no renderer");

        if (TeamAMaterial == null)
            Debug.LogError("No TeamAMaterial");
        if (TeamBMaterial == null)
            Debug.LogError("No TeamBMaterial");
        if (NeutralMaterial == null)
            Debug.LogError("No Neutral Material");

        if (TriggerCollider == null)
            Debug.LogError("No Trigger Collider");
    }

    private void Start()
    {
        mgr = PrisonDodgeballManager.Instance;

        IsHeld = false;
    }


    void Update()
    {
        // strange bug in shot selection scenario causing dodgeballs to scale strangely
        if (transform.localScale != Vector3.one)
            transform.localScale = Vector3.one;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float lastCollT = timeOfLastCollision;

        timeOfLastCollision = Time.time;
 
        if (IsHeld)
            return;

        //Debug.Log($"OnCollisionEnter hitting {collision.gameObject.name} in layer {collision.gameObject.layer} and looking for: {mgr.ArenaLayerLayerIndex} or: {mgr.MinionTeamALayerIndex} or: {mgr.MinionTeamBLayerIndex}");

        if(mgr == null)
        {
            mgr = PrisonDodgeballManager.Instance;
        }

        if (collision.gameObject.layer == mgr.ArenaLayerLayerIndex)
        {
            //Debug.Log("Arena hit");

            EventManager.TriggerEvent<BombBounceEvent, Vector3>(gameObject.transform.position);

            // tell manager about miss
            // a miss isn't a miss if this ball previously got a hit
            if (this.gameObject.layer != mgr.BallNeutralLayerIndex && !hasGottenHit)
            {
                mgr.INTERNAL_AnnounceMiss();
            }

            // go neutral
            INTERNAL_SetBallType(mgr.BallNeutralLayerIndex);

            hasGottenHit = false;

        }
        else if(collision.gameObject.layer == mgr.PrisonerTeamALayerIndex ||
            collision.gameObject.layer == mgr.PrisonerTeamBLayerIndex)
        {
            var minion = collision.gameObject.GetComponent<MinionScript>();

            if (minion != null)
            {

                //Debug.Log($"PRISONER MINION HIT! ispris:{minion.IsPrisoner} canbeRescued:{minion.CanBeRescued}");

                if (minion.IsPrisoner && minion.CanBeRescued)
                {
                    //Debug.Log($"ball layer: {this.gameObject.layer} minion layer: {collision.gameObject.layer} teamA:{mgr.MinionTeamALayerIndex} TeamB: { mgr.MinionTeamBLayerIndex} PrisA: {mgr.PrisonerTeamALayerIndex} PrisB: {mgr.PrisonerTeamBLayerIndex}");

                    if ((this.gameObject.layer == mgr.BallTeamALayerIndex &&
                        collision.gameObject.layer == mgr.PrisonerTeamALayerIndex) ||
                        (this.gameObject.layer == mgr.BallTeamBLayerIndex &&
                        collision.gameObject.layer == mgr.PrisonerTeamBLayerIndex))
                    {
                        // TODO check if prisoner is rescuable
                        INTERNAL_SetToTeam(minion.INTERNAL_TeamLayerIndex);
                        minion.INTERNAL_ReceiveRescue(this);
                    }
                }
  
            }
        }
        else if( collision.gameObject.layer == mgr.MinionTeamALayerIndex || 
            collision.gameObject.layer == mgr.MinionTeamBLayerIndex)
        {
            //Debug.Log("GOT THIS FAR");

            if (gameObject.layer == mgr.BallNeutralLayerIndex)
                return;

            var minion = collision.gameObject.GetComponent<MinionScript>();

            if(minion != null)
            {

                //Debug.Log("MINION HIT!");

                if(!minion.IsPrisoner && !minion.IsFreedPrisoner)
                {
                    //Debug.Log("MINION GOING PRISONER!");
                    minion.INTERNAL_ReceiveBallHit();

                    // tell manager about hit
                    mgr.INTERNAL_AnnounceHit();

                    hasGottenHit = true;

                    if(mgr.ThrowTestEnabled)
                    {
                        // go neutral
                        hasGottenHit = false;
                        INTERNAL_SetBallType(mgr.BallNeutralLayerIndex);
                        this.gameObject.SetActive(false);
                    }

                }
            }
        }
        
    }


    public void INTERNAL_SetBallType(int layerMask)
    {
        //Debug.Log($"SetBallType: {layerMask}");


        if(layerMask == mgr.BallNeutralLayerIndex)
        {
            //Debug.Log("setting ball to neutral");
            this.gameObject.layer = mgr.BallNeutralLayerIndex;
            rend.sharedMaterial = NeutralMaterial;
            TriggerCollider.enabled = true;
        }
        else if(layerMask == mgr.BallTeamALayerIndex)
        {
            //Debug.Log($"setting ball to team a {mgr.BallTeamALayerIndex}");
            this.gameObject.layer = mgr.BallTeamALayerIndex;
            rend.sharedMaterial = reverseColor ? TeamBMaterial : TeamAMaterial;
            TriggerCollider.enabled = false;
        }
        else if(layerMask == mgr.BallTeamBLayerIndex)
        {
            //Debug.Log($"setting ball to team b {mgr.BallTeamBLayerIndex}");
            this.gameObject.layer = mgr.BallTeamBLayerIndex;
            rend.sharedMaterial = reverseColor ? TeamAMaterial : TeamBMaterial;
            TriggerCollider.enabled = false;
        }
        else
        {
            Debug.LogError("Invalid ball type");
        }
    }


    public void INTERNAL_SetToTeam(int layerMask)
    {
        if (layerMask == mgr.MinionTeamALayerIndex)
        {
            //Debug.Log("Setting ball to team a");
            INTERNAL_SetBallType(mgr.BallTeamALayerIndex);
        }
        else if (layerMask == mgr.MinionTeamBLayerIndex)
        {
            //Debug.Log("setting ball to team b");

            INTERNAL_SetBallType(mgr.BallTeamBLayerIndex);
        }
        else
        {
            Debug.LogError("Unrecognized team layermask!");
        }
    }


    public void INTERNAL_OnTriggerStayFriend(Collider other)
    {
        //Debug.Log($"On trigger stay friend. Colliding with: {other.gameObject.name}");

        var minion = other.gameObject.GetComponent<MinionScript>();

        if(minion != null && minion.CanCollectBall)
        {
            //Debug.Log("Minion to receive ball");

            if (minion.INTERNAL_ReceiveBall(this))
            {
                INTERNAL_SetToTeam(minion.INTERNAL_TeamLayerIndex);
            }
        }
    }

}
