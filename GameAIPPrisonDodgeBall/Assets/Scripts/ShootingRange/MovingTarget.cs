using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class MovingTarget : MonoBehaviour
{
    public Rigidbody rbody { get; private set; }

    public ShootingRange mgr;

    float speed = 4f;

    public Vector2 ZRange = new Vector2(-6f, 13f);
    public Vector2 YRange = new Vector2(1f, 15f);

    public Vector2 SpeedRange = new Vector2(4f, 15f);

    public Material NeutralMaterial;
    public Material HitMaterial;

    public Renderer _renderer;

    float hitTime = 0f;

    public float HitFlashDuration = 0.1f;

    public float AbsXRange = 15f;


    public Vector3 DB_velocity;


    void Awake()
    {
        rbody = GetComponent<Rigidbody>();

        if (rbody == null)
        {
            Debug.LogError("no rigid body");
        }

        if(NeutralMaterial == null)
        {
            Debug.LogError("no neutral material");
        }

        if(HitMaterial == null)
        {
            Debug.LogError("no hit material");
        }

        if(_renderer == null)
        {
            Debug.LogError("no renderer");
        }


        rbody.useGravity = false;
    }

    private void Start()
    {
        var v = new Vector3(SpeedRange.x, 0f, 0f);
        v *= speed;
        rbody.AddForce(v, ForceMode.VelocityChange);
    }


    void FixedUpdate()
    {

        DB_velocity = rbody.velocity;

        if(_renderer.sharedMaterial == HitMaterial &&
            Time.timeSinceLevelLoad > (hitTime + HitFlashDuration))
        {
            _renderer.sharedMaterial = NeutralMaterial;
        }


        float abs_range = AbsXRange;

        if(transform.position.x > abs_range)
        {
            speed = Random.Range(SpeedRange.x, SpeedRange.y);

            var targetPos = new Vector3(-abs_range,
                Random.Range(YRange.x, YRange.y),
                Random.Range(ZRange.x, ZRange.y));

            var v = targetPos - transform.position;
            v.Normalize();
            v *= speed;
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
            rbody.AddForce(v, ForceMode.VelocityChange);
        }
        else if(transform.position.x < -abs_range)
        {
            speed = Random.Range(SpeedRange.x, SpeedRange.y);

            var targetPos = new Vector3(abs_range,
                Random.Range(YRange.x, YRange.y),
                Random.Range(ZRange.x, ZRange.y));

            var v = targetPos - transform.position;
            v.Normalize();
            v *= speed;
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
            rbody.AddForce(v, ForceMode.VelocityChange);
        }

    }


    public void SetSpeed(float s)
    {
        speed = s;
        var v = rbody.velocity;
        var vn = v.normalized;

        if(vn.magnitude < 0.5f)
        {
            vn = transform.right;
        }

        rbody.velocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;
        rbody.AddForce(vn * s, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("HIT");

        _renderer.sharedMaterial = HitMaterial;

        hitTime = Time.timeSinceLevelLoad;

        var proj = other.gameObject.GetComponentInParent<Projectile>();

        if(proj != null)
        {
            proj.AcceptHit();
        }

        mgr.INTERNAL_RecieveHit();
    }
}
