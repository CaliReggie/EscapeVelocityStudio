using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 10f;
    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
