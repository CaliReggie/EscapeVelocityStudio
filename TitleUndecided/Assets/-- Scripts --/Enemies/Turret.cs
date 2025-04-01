using UnityEngine;
using UnityEngine.ProBuilder;

public class Turret : MonoBehaviour
{
    [Range(0, 1)] public float lookSpeed = 0.3f;
    public static GameObject movewall;
    public GameObject Barrel;
    public GameObject Base;
    public Transform playerpos;
    public float distancetoplayer;
    public float dist = 10f;
    public Vector2 viewRange = new Vector2(45f, 45f);
    public Vector3 startingRot;
    
    private bool ranonce = false;

    private Vector3 movedWall;
    private Vector3 movedBase;
    private Vector3 movedBarrel;
    private Vector3 targetlookDir;
  

    void Start()
    {
        movewall = GameObject.Find("movewall");
        Barrel = GameObject.Find("barrel");
        Base = GameObject.Find("base");
        movedWall = new Vector3(movewall.transform.position.x, movewall.transform.position.y,
            movewall.transform.position.z - 1.1f);
        movedBase = new Vector3(movewall.transform.position.x, Base.transform.position.y, Base.transform.position.z);
        movedBarrel = new Vector3(Barrel.transform.position.x + 1.6f, Barrel.transform.position.y,
            Barrel.transform.position.z);
        
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
            if (!ranonce)
            {
                
                startingRot = Barrel.transform.eulerAngles;
                
                if (startingRot.y > 180)
                {
                    startingRot.y -= 360;
                    Debug.Log(startingRot.y);
                }

                if (startingRot.z > -180)
                {
                    startingRot.z -= 360;
                    Debug.Log(startingRot.z);
                }
                ranonce = true;
            }
            targetlookDir = startingRot;
            StartCoroutine(Lerpers.LerpTransform(Base.transform, movedBase, Lerpers.OutQuad(0.7f)));
            if (Base.transform.position == movedBase)
                
            {
                StartCoroutine(Lerpers.LerpTransform(Barrel.transform, movedBarrel, Lerpers.OutQuad(0.3f)));
                if (Barrel.transform.position == movedBarrel)
                {
                    RotateBarrel();

                }

            }

        }

    }

    void RotateBarrel()
    
    {
        Vector3 playerZOnly = new Vector3(playerpos.transform.position.x, playerpos.transform.position.y, playerpos.position.z);
        float xRot = Barrel.transform.localEulerAngles.x;

        Vector3 direction = playerpos.transform.position - Barrel.transform.position;
        
        targetlookDir = Quaternion.LookRotation(direction).eulerAngles;
        
        
        if (targetlookDir.y > 180)
        {
            targetlookDir.y -= 360;
        }
        if (targetlookDir.x > 180)
        {
            targetlookDir.x -= 360;
        }
        Debug.Log(targetlookDir.x);
        
        
        
        float ydiff = Mathf.Abs(startingRot.y) - Mathf.Abs(targetlookDir.y);
        float zdiff = Mathf.Abs(startingRot.x) - Mathf.Abs(targetlookDir.x);
        
        // Debug.Log("y diff = " +ydiff);
        // Debug.Log("z diff = " +zdiff);
        //
        if ((Mathf.Abs(ydiff) > viewRange.x ))
        {
            targetlookDir.y = startingRot.y;
        }
  
        if ((Mathf.Abs(zdiff) > viewRange.y ))
        {
            targetlookDir.x = startingRot.x;
        }
        //targetlookDir.y = Mathf.Lerp(Barrel.transform.eulerAngles.y, targetlookDir.y, lookSpeed);
        //targetlookDir.z = Mathf.Lerp(Barrel.transform.eulerAngles.z, targetlookDir.z, lookSpeed);
      
    
        Barrel.transform.eulerAngles=  targetlookDir;


    }
    
}