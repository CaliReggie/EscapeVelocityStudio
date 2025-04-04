using UnityEngine;
using System.Collections.Generic;
public class MeleeAttack : Weapon
{
    private static int lastMeleeAttack = -10;
    private static float lastUseTime;
    public float duration = 10f;
    private float _startTime;
    // Update is called once per frame
    void Start()
    {
        _startTime = Time.time;
        int index = Random.Range(0, 3);
        while  (Time.time - lastUseTime < 3f && lastMeleeAttack == index)
        {
            index = Random.Range(0, 3);
        }
        GetComponent<Animator>().SetInteger("Index", index);
        lastMeleeAttack = index;
        lastUseTime = Time.time;
    }
    void Update()
    {
        if (Time.time - _startTime > duration)
        {
            Destroy(gameObject);
        }
    }
    void EndOfAttack()
    {
        Destroy(gameObject);
    }
}
