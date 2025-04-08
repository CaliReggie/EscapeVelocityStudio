using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum EEquippableWeapon
{
    DamageDisk,
    TeleportDisk,
    StickyDisk,
    Melee,
}
public abstract class Weapon : MonoBehaviour
{
    public EEquippableWeapon weaponType;
    public LayerMask enemyMask;
    public float damageAmt = 5f;
    public float lockedInAttackDur;
    protected virtual void CheckForEnemy(Collider other)
    {
        if (Utils.IsLayerInLayerMask(other.gameObject.layer, enemyMask))
        {
            other.gameObject.GetComponent<Enemy>().TakeDamage(damageAmt);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        CheckForEnemy(other);
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        CheckForEnemy(other.collider);
    }
}
