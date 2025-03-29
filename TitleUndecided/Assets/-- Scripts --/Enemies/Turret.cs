using UnityEngine;

public class Turret : MonoBehaviour
{
    public static GameObject movewall;
    public GameObject Barrel;
    public GameObject Base;
    public Transform playerpos;
    public float distancetoplayer;
    public float dist = 10f;
    
    private Vector3 movedWall;
    private Vector3 movedBase;
    private Vector3 movedBarrel;
    private float maxBarrelRotLeft;
    private float maxBarrelRotRight;
    void Start()
    {
        movewall = GameObject.Find("movewall");
        Barrel = GameObject.Find("barrel");
        Base = GameObject.Find("base");
        movedWall = new Vector3(movewall.transform.position.x, movewall.transform.position.y, movewall.transform.position.z - 1.1f);
        movedBase = new Vector3(movewall.transform.position.x , Base.transform.position.y, Base.transform.position.z);
        movedBarrel = new Vector3(Barrel.transform.position.x + 1.6f, Barrel.transform.position.y, Barrel.transform.position.z);
        maxBarrelRotLeft = Barrel.transform.rotation.z - 60f;
        maxBarrelRotRight = Barrel.transform.rotation.z + 60f;


    }
    
    void Update()
    {
        distancetoplayer = Vector3.Distance(transform.position, playerpos.position);
        if (distancetoplayer < dist)
        {
            TurrentInit();
            
        }
        
    }

    public void TurrentInit()
    {
        StartCoroutine(Lerpers.LerpTransform(movewall.transform, movedWall, Lerpers.OutQuad(1f)));
        if (movewall.transform.position == movedWall)
        {
            StartCoroutine(Lerpers.LerpTransform(Base.transform, movedBase, Lerpers.OutQuad(0.7f)));
            if (Base.transform.position == movedBase)
            {
                StartCoroutine(Lerpers.LerpTransform(Barrel.transform, movedBarrel, Lerpers.OutQuad(0.3f)));
                if (Barrel.transform.position == movedBarrel)
                {
                    TurretShoot();
                    
                }
                
            }
            
        }

    }

    public void TurretShoot()
    {
        while (Barrel.transform.rotation.z < maxBarrelRotRight && Barrel.transform.rotation.z > maxBarrelRotLeft)
        {
            Barrel.transform.LookAt(playerpos.position);
            
        }
        
    }
}
