using UnityEngine;
using System.Collections.Generic;
public class MeleeAttack : Weapon
{
    private static int lastMeleeAttack = -10;
    private static float lastUseTime;
    public float duration = 10f;
    public bool swinging = false;
    private float _startTime;
    // Update is called once per frame
    private HashSet<Collider> hitColliders = new HashSet<Collider>();
    void Start()
    {
        _startTime = Time.time;
        if (GetComponent<Animator>().parameters.Length >0)
        {
            int index = Random.Range(0, 2);
            while  (Time.time - lastUseTime < 3f && lastMeleeAttack == index)
            {
                index = Random.Range(0, 2);
            }
            GetComponent<Animator>().SetInteger("Index", index);
            lastMeleeAttack = index;
            lastUseTime = Time.time;
        }
    }
    void Update()
    {
        if (Time.time - _startTime > duration)
        {
            Destroy(gameObject);
        }

        if (swinging)
        {
            if (hitColliders.Count > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    base.CheckForEnemy(collider);
                }
                hitColliders.Clear();
            }
        }
    }
    void EndOfAttack()
    {
        Destroy(gameObject);
    }

    void EndOfCharge()
    {
        swinging = true;
    }

    protected override void OnTriggerEnter(Collider other)
    { 
        hitColliders.Add(other);
    }
}
