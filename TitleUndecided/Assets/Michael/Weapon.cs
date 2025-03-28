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
    public float lockedInAttackDur;
    public LayerMask enemyMask;
    public float damageAmt = 5f;

    protected virtual void CheckForEnemy(Collision collision)
    {
        if (Utils.IsLayerInLayerMask(collision.gameObject.layer, enemyMask))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damageAmt);
        }
    }
}
