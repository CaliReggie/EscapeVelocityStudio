using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 5.0f;
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        
        
    }
}
