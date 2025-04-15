using UnityEngine;

public class bullet : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject target;
    public float speed = 5.0f;
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        
        
    }
}
