using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private GameObject barrelBase;
    
    [SerializeField] private GameObject turretBase;
    public GameObject shotpoint;
    public GameObject bullet;
    
    [SerializeField] private Transform playerPos;
    
    [Header("Detection Settings")]
    
    [SerializeField] private float targetDistance = 10f;
    
    [SerializeField] private Vector2 targetDiameter = new (135f, 45f);
    
    [Range(0, 1)] [SerializeField] private float lookSpeed = 0.3f;
    
    [Header("Behaviour Settings")]
    
    [SerializeField] private bool radiusRelativeToBase = true;
    
    [SerializeField] private bool returnsToBaseRot = true;
    private bool canShoot = true;
    
    [Header("Dynamic")]
    
    [SerializeField] private float playerDistance;
    
    [SerializeField] private Vector3 currentRotation;
    
    [SerializeField] private Quaternion targetRotation;
    
    [Header("Debug")]
    
    [SerializeField] private Color fullRadiusColor = Color.white;
    
    [SerializeField] private Color targetRadiusColor = Color.red;
    
    private void Start()
    {
        currentRotation = barrelBase.transform.rotation.eulerAngles;
        
        targetRotation = barrelBase.transform.rotation;
    }
    
    private void Update()
    {
        if (playerPos == null) return;
        
        playerDistance = Vector3.Distance(transform.position, playerPos.position);
        
        if (playerDistance <= targetDistance)
        {
            RotateBarrel();
            if (canShoot)
            {
                Shoot();
            }
        }
        else
        {
            if (returnsToBaseRot)
            {
                targetRotation = turretBase.transform.rotation;
                    
                barrelBase.transform.rotation = Quaternion.Slerp(barrelBase.transform.rotation, targetRotation, lookSpeed);
                
                currentRotation = barrelBase.transform.rotation.eulerAngles;
            }
        }
    } 
    
    private bool anyAngleClampedThisFrame;
    private void RotateBarrel()
    {
        Vector3 targetDirection = playerPos.transform.position - barrelBase.transform.position;
        
        Quaternion targetDirRot = Quaternion.LookRotation(targetDirection);
        
        Vector3 targetRotEuler = targetDirRot.eulerAngles;
        
        if (returnsToBaseRot)
        {
            if (radiusRelativeToBase)
            {
                targetRotEuler = ClampEulerRotToBase(turretBase.transform.eulerAngles, targetRotEuler, targetDiameter / 2);
            }
            else
            {
                targetRotEuler = ClampEulerRotToBase(currentRotation, targetRotEuler, targetDiameter / 2);
            }
            
            if (anyAngleClampedThisFrame)
            {
                targetRotEuler = turretBase.transform.eulerAngles;
            }
        }
        else 
        {
            if (radiusRelativeToBase)
            {
                targetRotEuler = ClampEulerRotToCurrent(currentRotation, turretBase.transform.eulerAngles, targetRotEuler,
                    targetDiameter / 2);
            }
            else
            {
                targetRotEuler = ClampEulerRotToCurrent(currentRotation, currentRotation, targetRotEuler,
                    targetDiameter / 2);
            }
            
            if (anyAngleClampedThisFrame)
            {
                targetRotEuler = currentRotation;
            }
        }
        
        anyAngleClampedThisFrame = false;
        
        targetRotation = Quaternion.Euler(targetRotEuler);
        
        barrelBase.transform.rotation = Quaternion.Slerp(barrelBase.transform.rotation, targetRotation, lookSpeed);
        
        currentRotation = barrelBase.transform.rotation.eulerAngles;
        
        return;
        
        Vector3 ClampEulerRotToBase(Vector3 baseEuler, Vector3 targetEuler, Vector2 limits)
        {
            Vector3 a = baseEuler;
            
            Vector3 b = targetEuler;
            
            b.y = ClampAngleToBase(a.y, b.y , -limits.x, limits.x);
            
            b.x = ClampAngleToBase(a.x, b.x, -limits.y, limits.y);
            return b;
        }
        
        Vector3 ClampEulerRotToCurrent(Vector3 currentEuler, Vector3 baseEuler, Vector3 targetEuler, Vector2 limits)
        {
            Vector3 a = baseEuler;
            
            Vector3 b = targetEuler;
            
            b.y = ClampAngleToCurrent(currentEuler.y, a.y, b.y , -limits.x, limits.x);
            
            b.x = ClampAngleToCurrent(currentEuler.x, a.x, b.x, -limits.y, limits.y);
            
            return b;
        }
        
        float ClampAngleToBase(float baseDir, float targetDir, float minAngle, float maxAngle)
        {
            float angle = EulerAngle(baseDir, targetDir);
            
            bool isBetween = angle > minAngle && angle < maxAngle;
            
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            
            if (isBetween)
            {
                return baseDir + angle;
            }
            else
            {
                anyAngleClampedThisFrame = true;
                
                return baseDir;
            }
        }
        
        float ClampAngleToCurrent(float current, float baseDir, float targetDir, float minAngle, float maxAngle)
        {
            float angle = EulerAngle(baseDir, targetDir);
            
            bool isBetween = angle > minAngle && angle < maxAngle;
            
            angle = Mathf.Clamp(angle, minAngle, maxAngle);


            
            if (isBetween)
            {
                return baseDir + angle;
            }
            else
            {
                anyAngleClampedThisFrame = true;
                
                return current;
            }
        }
        
        float EulerAngle(float dirOne, float dirTwo)
        {
            float angle = dirTwo - dirOne;
            
            angle = Mathf.Repeat(angle, 360f);
            
            if (angle > 180f) {angle -= 360f;} 
            
            return angle;
        }
    }
        
    // If radius is relative to base, we draw a radius from barrel base in the 4 relative angles that go from
    // the base rotation plus the relative target radius angles
    // If not, this radius is drawn relative to the current rotation of the barrel base
    private void OnDrawGizmos()
    {
        Gizmos.color = fullRadiusColor;
        
        if (turretBase == null) return; 
        if (barrelBase == null) return;
        
        Gizmos.DrawWireSphere(barrelBase.transform.position, targetDistance);
        
        float xRadius = targetDiameter.x / 2;
        
        float yRadius = targetDiameter.y / 2;
        
        Vector3[] angles = new Vector3[4];
        
        Vector3 rotation = Vector3.zero;
        
        if (radiusRelativeToBase)
        {
            rotation = turretBase.transform.rotation.eulerAngles;
            
            angles[0] = new Vector3(rotation.x + yRadius, rotation.y + xRadius, rotation.z);
            
            angles[1] = new Vector3(rotation.x - yRadius, rotation.y + xRadius, rotation.z);
            
            angles[3] = new Vector3(rotation.x + yRadius, rotation.y - xRadius, rotation.z);
            
            angles[2] = new Vector3(rotation.x - yRadius, rotation.y - xRadius, rotation.z);
        }
        else
        {
            rotation = barrelBase.transform.rotation.eulerAngles;
            
            angles[0] = new Vector3(rotation.x + yRadius, rotation.y + xRadius, rotation.z);
            
            angles[1] = new Vector3(rotation.x - yRadius, rotation.y + xRadius, rotation.z);
            
            angles[3] = new Vector3(rotation.x + yRadius, rotation.y - xRadius, rotation.z);
            
            angles[2] = new Vector3(rotation.x - yRadius, rotation.y - xRadius, rotation.z);
        }
        
        Gizmos.color = targetRadiusColor;
        
        //drawing arms of the radius
        for (int i = 0; i < angles.Length; i++)
        {
            Vector3 dir = Quaternion.Euler(angles[i]) * Vector3.forward * targetDistance;
            
            Gizmos.DrawRay(barrelBase.transform.position, dir);
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        playerPos = newTarget;
    }
    void Shoot()
    {
        Instantiate(bullet, shotpoint.transform.position, shotpoint.transform.rotation);
        StartCoroutine(Cooldown());

    }

    private IEnumerator Cooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(0.2f);
        canShoot = true;

    }
 
}

