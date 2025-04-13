using System;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Disk : Weapon
{
    [Header("Basic Disk Data")]
    public AudioClip[] ricochetAudioClips;
    public float speed = 15;
    public float lifeSpan = 5f;
    [Header("Ricochet")]
    public bool ricochet = false;
    public int maxRicochetCount = 0;
    public float rayDist = 0.6f;
    public LayerMask ricochetMask;
    protected GameObject recentHitObject;
    protected float recentHitTime;
    protected float startTime;
    protected Rigidbody rb;
    protected int ricochetCount = 0;
    public bool homing;
    public GameObject currentTarget;

    protected virtual void Awake()
    {
        startTime = Time.time;
        rb = GetComponent<Rigidbody>();
        base.Awake();
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        TimeCheck();
    }

    protected virtual void Start()
    {
        transform.parent = null;
        Vector3 playerMovementForce = Vector3.zero; 
        Vector3 playerVelocity = PlayerEquipabbles.S.GetComponent<Rigidbody>().linearVelocity;
        playerVelocity = Vector3.Project(playerVelocity, transform.forward);
        if (Vector3.Dot(playerVelocity, transform.forward) > 0)
        {
            playerMovementForce = playerVelocity;
        }
        rb.linearVelocity = playerMovementForce + transform.forward * speed;
    }
    protected virtual void TimeCheck()
    {
        if (lifeSpan + startTime < Time.time)
        {
            DiskEnd();
        }
    }

    void FixedUpdate()
    {
        Movement();
        if (ricochet)
        {
            RayRicochetCheck();
        }
    }

    protected virtual void Movement()
    {
        if (rb.linearVelocity.magnitude > 0.05f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
        rb.AddForce(new Vector3(0f, -5f, 0f), ForceMode.Acceleration);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if ((recentHitObject == collision.collider.gameObject && ((Time.time - recentHitTime) < 0.25f) || collision.gameObject == gameObject))
        {
            return;
        } 
        base.OnCollisionEnter(collision);
        if (Utils.IsLayerInLayerMask(collision.gameObject.layer, ricochetMask))
        {
            if (ricochet)
            {
                Ricochet(collision.contacts[0].normal);
            }
            else
            {
                DiskEnd();
            }
        }
        else
        {
            DiskEnd();
        }
    }

    protected virtual void RayRicochetCheck()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDist, ricochetMask))
        {
            if (recentHitObject == hit.collider.gameObject && ((Time.time - recentHitTime) < 0.25f) || hit.collider.gameObject.layer == 13)
            {
                return;
            }
            Ricochet(hit.normal);
            recentHitObject = hit.collider.gameObject;
            if (Utils.IsLayerInLayerMask(hit.collider.gameObject.layer, enemyMask))
            {
                CheckForEnemy(hit.collider);
            }
            else 
            {
                if (ricochet)
                {
                    audioSource.clip = ricochetAudioClips[Random.Range(0, ricochetAudioClips.Length)];
                    audioSource.Play();
                }
            }
        }
    }

    protected virtual void Ricochet(Vector3 hitNormal)
    {
        ricochetCount++;
        if (ricochetCount >= maxRicochetCount)
        {
            ricochet = false;
            DiskEnd();
        }
        Vector3 reflectDirection= Vector3.Reflect(transform.forward, hitNormal);
        transform.forward = reflectDirection;
        float currentSpeed = rb.linearVelocity.magnitude;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(transform.forward * currentSpeed, ForceMode.Impulse);
        recentHitTime = Time.time;
    }
    protected virtual void DiskEnd()
    {
        Destroy(gameObject);
    }
    public virtual void EnemyInRange(GameObject target)
    {
        if (!homing)
        {
            return;
        }
        if (currentTarget == null)
        {
            currentTarget = target;
        }
        transform.forward = Vector3.Slerp(transform.forward, target.transform.position - transform.position, 10 * Time.deltaTime);
        float currentSpeed = rb.linearVelocity.magnitude;
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    public virtual void EnemyOutRange(GameObject target)
    {
        if (currentTarget == target)
        {
            currentTarget = null;
        }
    }
}
