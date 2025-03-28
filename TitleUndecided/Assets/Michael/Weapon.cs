using UnityEngine;

public enum EEquippableWeaponType
{
    DamageDisk,
    TeleportDisk,
    StickyDisk,
    Melee,
}
public abstract class Weapon : MonoBehaviour
{
    public EEquippableWeaponType weaponType;
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
