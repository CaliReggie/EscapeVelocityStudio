using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform target;

    public float dist;

    public float firedist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firedist = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        dist = Vector3.Distance(transform.position, target.position);
        if (dist < firedist)
        {
            Debug.Log("Shooter");
            LookAtPlayer();
        }
        
    }

    void LookAtPlayer()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = rotation;
        
    }

}
