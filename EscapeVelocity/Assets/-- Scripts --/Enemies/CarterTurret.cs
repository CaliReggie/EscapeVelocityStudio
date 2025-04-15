using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;
using System.Collections;
using System.ComponentModel;
using UnityEngine.InputSystem;
public class CarterTurret : MonoBehaviour
{
    [Header("References")]
    
    public static GameObject movewall;
    
    public GameObject Barrel;
    
    public GameObject Base;
    public GameObject bullet;
    public Transform shotpoint;
    
    public Transform playerpos;
    
    [Header("Detection Settings")]
    
    public float dist = 10f;

    public Vector2 viewRadius = new Vector2(135f, 45f);
    
    [Range(0, 1)] public float lookSpeed = 0.3f;
    
    [Header("Behaviour Settings")]
    
    public bool returnsToStartRot = true;
    
    [Header("Dynamic")]
    
    public float distancetoplayer;

    private float turretScale;

    private float fireSpeed = 1f;
    
    public Quaternion startingRotation;
    
    public Vector3 currentRotation;
    
    public Quaternion targetRotation;
    
    //Non serialized or private below
    private bool ranonce = false;
    private Vector3 wallPosition;
    private Vector3 basePosition;
    private Vector3 barrelPosition;
    private Vector3 movedWall; 
    Vector3 movedBase;
    private Vector3 movedBarrel;
    private bool outOfBarrel = false;
    private bool inbarrel = true;
    public bool canShoot = false;
    public bool In = false;
    
    
  

    void Start()
    {
        // viewRadius.x = Mathf.Clamp(viewRadius.x, 0, 179);
        // viewRadius.y = Mathf.Clamp(viewRadius.y, 0, 179);
        
        movewall = GameObject.Find("movewall");
        Barrel = GameObject.Find("barrel");
        Base = GameObject.Find("base");
        wallPosition = movewall.transform.position;
        basePosition = Base.transform.position;
        barrelPosition = Barrel.transform.position;
        turretScale = this.transform.localScale.x;
        Debug.Log(turretScale);
        
        movedWall = movewall.transform.position - movewall.transform.forward * 1.1f * turretScale;
        movedBase = Base.transform.position + Base.transform.right * 1.3f * turretScale;
        float yRot = Barrel.transform.eulerAngles.y;
        if (Mathf.Approximately(yRot, 0f) || Mathf.Approximately(yRot, 180f))
        {
            movedBarrel = Barrel.transform.position + Barrel.transform.forward * 1.4f * turretScale;
        }
        else
        {
            movedBarrel = Barrel.transform.position + Barrel.transform.forward * 1.6f * turretScale;
        }
    }
    

    void Update()
    {
    
        if (GameInputManager.Instance.PlayerInput != null && playerpos == null)
        {
            
            playerpos = GameInputManager.Instance.PlayerInput.transform.GetComponentInChildren<PlayerMovement>().transform;

        }
        if (playerpos == null) return;
    
        distancetoplayer = Vector3.Distance(transform.position, playerpos.position);
        if (outOfBarrel)
        {
            RotateBarrel();
        }
        if (distancetoplayer < dist && inbarrel)
        {
            TurrentInit();

        }
        
        if (distancetoplayer > dist && outOfBarrel)
        {
            TurretRetract();
        }

        if (!In && canShoot)
        {
            Shoot(); 
        }
        

    }

    public void TurrentInit()
    {
        StartCoroutine(Lerpers.LerpTransform(movewall.transform, movedWall, Lerpers.OutQuad(1f)));
        if (movewall.transform.position == movedWall)
        {
            if (!ranonce)
            {

                startingRotation = Barrel.transform.rotation;

                ranonce = true;
            }
            
            StartCoroutine(Lerpers.LerpTransform(Base.transform, movedBase, Lerpers.OutQuad(0.7f)));
            if (Base.transform.position == movedBase)
            {
                StartCoroutine(Lerpers.LerpTransform(Barrel.transform, movedBarrel, Lerpers.OutQuad(0.3f)));
                if (Barrel.transform.position == movedBarrel)
                {
                    Debug.Log(Base.transform.position);
                    Debug.Log(movedBarrel);
                    inbarrel = false;
                    outOfBarrel = true;
                    In = false;
                    canShoot = true;
                    //if(canShoot) Shoot();

                }

            }
        }
    }

void RotateBarrel()
    {
        Vector3 targetDir = playerpos.transform.position - Barrel.transform.position;
        
        Quaternion targetDirRot = Quaternion.LookRotation(targetDir);
        
        Vector3 targetRotEuler = targetDirRot.eulerAngles;
        
        if (returnsToStartRot)
        {
            targetRotEuler = ClampEulerRot(startingRotation.eulerAngles, targetRotEuler, viewRadius / 2,
                false);
        }
        else 
        {
            targetRotEuler = ClampEulerRot(startingRotation.eulerAngles, targetRotEuler, viewRadius / 2,
                true, currentRotation);
        }
        
        targetRotation = Quaternion.Euler(targetRotEuler);
        
        Barrel.transform.rotation = Quaternion.Slerp(Barrel.transform.rotation, targetRotation, lookSpeed);
        
        currentRotation = Barrel.transform.rotation.eulerAngles;
        
        return;
        
        float EulerAngle(float dirOne, float dirTwo)
        {
            float angle = dirTwo - dirOne;
            
            angle = Mathf.Repeat(angle, 360f);
            
            if (angle > 180f) {angle -= 360f;} 
            
            return angle;
        }
        
        float ClampAngleToCurrent(float dirOne, float dirTwo, float minAngle, float maxAngle, float current)
        {
            float angle = EulerAngle(dirOne, dirTwo);
            
            bool isBetween = angle > minAngle && angle < maxAngle;
            
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            
            if (isBetween) { return dirOne + angle; }
            
            else
            {
                return current;
            }
        }
        
        float ClampAngleToBase(float dirOne, float dirTwo, float minAngle, float maxAngle)
        {
            float angle = EulerAngle(dirOne, dirTwo);
            
            bool isBetween = angle > minAngle && angle < maxAngle;
            
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            
            if (isBetween) { return dirOne + angle; }
            else { return dirOne; }
        }
        
        Vector3 ClampEulerRot(Vector3 baseEuler, Vector3 targetEuler, Vector2 limits, bool clampToCurrent, 
            Vector3 current = default)
        {
            Vector3 a = baseEuler;
            
            Vector3 b = targetEuler;
            
            if (clampToCurrent)
            {
                b.x = ClampAngleToCurrent(a.x, b.x, -limits.y, limits.y, current.x);
            
                b.y = ClampAngleToCurrent(a.y, b.y , -limits.x, limits.x, current.y);
            }
            else
            {
                b.x = ClampAngleToBase(a.x, b.x, -limits.y, limits.y);
            
                b.y = ClampAngleToBase(a.y, b.y , -limits.x, limits.x);
            }
            
            return b;
        }
    }

    void TurretRetract()
    {
        StartCoroutine(Lerpers.LerpTransform(Base.transform, basePosition, Lerpers.OutQuad(0.5f)));
        if (Base.transform.position == basePosition)
            
        {       
                StartCoroutine(Lerpers.LerpTransform(Barrel.transform, barrelPosition, Lerpers.OutQuad(0.2f)));
                if (Barrel.transform.position == barrelPosition)
                {
                    StartCoroutine(Lerpers.LerpTransform(movewall.transform, wallPosition, Lerpers.OutQuad(0.5f)));
                    inbarrel = true;
                    outOfBarrel = false;
                    In = true;
                }
        }

    }

    void Shoot()
    {
        Instantiate(bullet, shotpoint.transform.position, shotpoint.transform.rotation);
        StartCoroutine(Cooldown());

    }

    private IEnumerator Cooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireSpeed);
        canShoot = true;

    }
 
}

