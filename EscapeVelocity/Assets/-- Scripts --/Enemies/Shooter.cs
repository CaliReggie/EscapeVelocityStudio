using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform target;
    private bool returnsToStartRot = true;
    public Vector2 viewRadius = new Vector2(135f, 45f);
    private Vector3 startingRotation;
    public Quaternion targetRotation;
    public Vector3 currentRotation;
    public GameObject bulletPrefab;
    


    public float dist;

    public float firedist;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firedist = 15f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameInputManager.Instance.PlayerInput != null && target == null)
        {
            
            target = GameInputManager.Instance.PlayerInput.transform.GetComponentInChildren<PlayerMovement>().transform;

        }
        if (target == null) return;
        dist = Vector3.Distance(transform.position, target.position);
        if (dist < firedist)
        {
            Debug.Log("Shooter");
            LookAtPlayer();
            barrel.inRange = true;

        }
        else
        {
            barrel.inRange = false;
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
