using UnityEngine;

public class AirSlash : Weapon
{
    public float speed = 20f;
    public float duration = 3f;
    private float timer;
    public bool secondary = false;
    public Vector3 direction;
    void Start()
    {
        transform.SetParent(null, true);
        if (!secondary)
        {
            direction = -transform.right;
        }
        else
        {
            direction = transform.up;
        }
        Vector3 playerMovementForce = Vector3.zero; 
        Vector3 playerVelocity = PlayerEquipabbles.S.GetComponent<Rigidbody>().linearVelocity;
        if (playerVelocity.x * direction.x > 0)
        {
            playerMovementForce.x = playerVelocity.x;
        }

        if (playerVelocity.y * direction.y > 0)
        {
            playerMovementForce.y = playerVelocity.y;
        }
        if (playerVelocity.z * direction.z > 0)
        {
            playerMovementForce.z = playerVelocity.z;
        }

        GetComponent<Rigidbody>().linearVelocity = direction * speed + playerMovementForce;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
