using UnityEngine;

public class AirSlash : Weapon
{
    public float speed = 20f;
    public float duration = 3f;
    private float timer;
    public bool secondary = false;
    void Start()
    {
        transform.SetParent(null, true);
        if (!secondary)
        {
            GetComponent<Rigidbody>().linearVelocity = -transform.right * speed;
        }
        else
        {
            GetComponent<Rigidbody>().linearVelocity = transform.up * speed;
        }
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
