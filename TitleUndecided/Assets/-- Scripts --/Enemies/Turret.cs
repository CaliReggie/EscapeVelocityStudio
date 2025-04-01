using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;

public class Turret : MonoBehaviour
{
    [Header("References")]
    
    public static GameObject movewall;
    
    public GameObject Barrel;
    
    public GameObject Base;
    
    public Transform playerpos;
    
    [Header("Detection Settings")]
    
    public float dist = 10f;

    public Vector2 viewRadius = new Vector2(45f, 45f);
    
    [Range(0, 1)] public float lookSpeed = 0.3f;
    
    [Header("Behaviour Settings")]
    
    public bool returnsToStartRot = true;
    
    [Header("Dynamic")]
    
    public float distancetoplayer;
    
    public Quaternion startingRot;
    
    [FormerlySerializedAs("targetRot")]
    public Quaternion currentTargetRot;
    
    //Non serialized or private below
    private bool ranonce = false;
    private Vector3 movedWall;
    private Vector3 movedBase;
    private Vector3 movedBarrel;
    
  

    void Start()
    {
        // viewRadius.x = Mathf.Clamp(viewRadius.x, 0, 179);
        // viewRadius.y = Mathf.Clamp(viewRadius.y, 0, 179);
        
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

                startingRot = Barrel.transform.rotation;

                ranonce = true;
            }
            
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
        Vector3 targetDir = playerpos.transform.position - Barrel.transform.position;
        
        Quaternion targetDirRot = Quaternion.LookRotation(targetDir);
        
        Vector3 targetRotEuler = targetDirRot.eulerAngles;
        
        targetRotEuler = ClampEuler(startingRot.eulerAngles, targetRotEuler, viewRadius / 2);
        
        currentTargetRot = Quaternion.Euler(targetRotEuler);
        
        Barrel.transform.rotation = Quaternion.Slerp(Barrel.transform.rotation, currentTargetRot, lookSpeed);
        
        return;
        
        float ClampAngle(float angle, float minAngle, float maxAngle)
        {
            angle = Mathf.Repeat(angle, 360f); 
            
            if (angle > 180f) {angle -= 360f;} 
            
            bool isBetween = angle > minAngle && angle < maxAngle;
            
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            
            if (isBetween) {return angle;}
            else
            {
                if (returnsToStartRot)
                {
                    return 0;
                }
                else
                {
                    return angle;
                }
            }
        }
        
        // float NewClampAngle(float dirOne, float dirTwo, float minAngle, float maxAngle, float fallbackAngle = -180)
        // {
        //     float angle = dirTwo - dirOne;
        //     
        //     angle = Mathf.Repeat(angle, 360f); 
        //     
        //     if (angle > 180f) {angle -= 360f;} 
        //     
        //     bool isBetween = angle > minAngle && angle < maxAngle;
        //     
        //     angle = Mathf.Clamp(angle, minAngle, maxAngle);
        //     
        //     if (isBetween) {return dirOne + angle;}
        //     else
        //     {
        //         if (fallbackAngle <= -180)
        //         {
        //             return dirOne;
        //         }
        //         else
        //         {
        //             return fallbackAngle;
        //         }
        //     }
        // }
        
        Vector3 ClampEuler(Vector3 startRot, Vector3 targetRot, Vector2 limits)
        {
            Vector3 start = startRot;
            
            Vector3 target = targetRot;
            
            target.x = ClampAngle(target.x - start.x, -limits.y, limits.y) + start.x;
            
            target.y = ClampAngle(target.y - start.y, -limits.x, limits.x) + start.y;
            
            // if (returnsToStartRot)
            // {
            //     target.x = NewClampAngle(start.x, target.x, -limits.y, limits.y);
            //
            //     target.y = NewClampAngle(start.x, target.x , -limits.x, limits.x);
            // }
            // else
            // {
            //     target.x = NewClampAngle(start.x, target.x, -limits.y, limits.y, Barrel.transform.rotation.eulerAngles.x);
            //
            //     target.y = NewClampAngle(start.x, target.x , -limits.x, limits.x, Barrel.transform.rotation.eulerAngles.y);
            // }
            
            return target;
        }
    }
    
}