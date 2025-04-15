using UnityEngine;
using System.Collections;

public class barrel : MonoBehaviour
{
    public Transform target;
    private bool returnsToStartRot = true;
    public Vector2 viewRadius = new Vector2(135f, 45f);
    private Vector3 startingRotation;
    public Quaternion targetRotation;
    public Vector3 currentRotation;
    static public bool inRange = false;
    public bool canShoot = true;
    private bool barrel1shoot = true;
    public GameObject bulletPrefab;
    public GameObject barrel1;
    public GameObject barrel2;
    public float fireRate = 0.5f;
    public float dist;
    public float firedist;

    void Update()
    {
        if (inRange)
        {
            if (canShoot)
            {
                if (barrel1shoot)
                {
                    Shoot(barrel1);
                }
                else
                {
                    Shoot(barrel2);
                    
                }
            }
        }
    }

    void Shoot(GameObject barrel)
    {
        GameObject bullet = Instantiate(bulletPrefab, barrel.transform.position, barrel.transform.rotation);
        Vector3 direction = (target.position - barrel.transform.position).normalized;
        bullet.transform.forward = direction;
        StartCoroutine(Cooldown());
    }
    private IEnumerator Cooldown()
    {
        if (barrel1shoot)
        {
            barrel1shoot = false;
        }
        else
        {
            barrel1shoot = true;
        }
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;

    }

}


