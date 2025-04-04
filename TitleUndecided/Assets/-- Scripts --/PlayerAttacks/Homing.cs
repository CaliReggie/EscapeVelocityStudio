using UnityEngine;

public class Homing : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject != null)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("whatIsEnemy"))
            {
                GetComponentInParent<Disk>().EnemyInRange(other.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != null)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("whatIsEnemy"))
            {
                GetComponentInParent<Disk>().EnemyOutRange(other.gameObject);
            }
        }
    }
}
