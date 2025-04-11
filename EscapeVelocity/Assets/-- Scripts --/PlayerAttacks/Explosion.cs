using System;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public LayerMask enemyMask;
    public float expForce = 100f;
    public float expDamage = 15f;
    public float lifeTime = 2f;
    public float startTime;
    void OnTriggerEnter(Collider other)
    {
        if (Utils.IsLayerInLayerMask(other.gameObject.layer, enemyMask))
        {
            Vector3 direction = other.transform.position - transform.position;
            float radius = direction.magnitude;
            other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(expForce, transform.position, radius);
            try {other.gameObject.GetComponent<Enemy>().TakeDamage(expDamage);}
            catch (NullReferenceException e)
            {
               try {other.gameObject.GetComponentInParent<Enemy>().TakeDamage(expDamage);}
               catch (NullReferenceException e2)
               {
                   return;
               }
            }
        }
    }

    void Awake()
    {
        startTime = Time.time;
    }
    void Update()
    {
        if (Time.time - startTime > lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
