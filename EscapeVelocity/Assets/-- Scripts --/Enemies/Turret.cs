using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Serialization;
using System.Collections;

public class Turret : MonoBehaviour
{
    [Header("References")]
    
    public GameObject Base;
    
    public GameObject BarrelBase;
    
    public Transform playerpos;
    
    [Header("Detection Settings")]
    
    public float dist = 10f;

    public Vector2 viewRadius = new Vector2(135f, 45f);
    
    [Range(0, 1)] public float lookSpeed = 0.3f;
    
    [Header("Behaviour Settings")]
    
    public bool returnsToStartRot = true;
    
    [Header("Dynamic")]
    
    public float distancetoplayer;
    
    public Quaternion localStartingRot;
    
    public Vector3 currentRotation;
    
    public Quaternion targetRotation;


    private void OnEnable()
    {
        localStartingRot = BarrelBase.transform.localRotation;
    }
    
    //returns the starting rotation (stores as local) to world rotation
    private Quaternion WorldStartingRot()
    {
        Quaternion worldRotation = Base.transform.rotation * localStartingRot;
        
        return worldRotation;
    }


    void Update()
    {
        if (playerpos == null) { return; }
        
        distancetoplayer = Vector3.Distance(playerpos.position, BarrelBase.transform.position);
        
        if (distancetoplayer < dist)
        {
            RotateBarrel();
        }
        else
        {
            if (returnsToStartRot)
            {
                targetRotation = Quaternion.Euler(WorldStartingRot().eulerAngles);
                
                BarrelBase.transform.rotation = Quaternion.Slerp(BarrelBase.transform.rotation, targetRotation,
                    lookSpeed);
                
                currentRotation = BarrelBase.transform.rotation.eulerAngles;
            }
        }
    }

    void RotateBarrel()
    {
        Vector3 targetDir = playerpos.transform.position - BarrelBase.transform.position;
        
        Quaternion targetDirRot = Quaternion.LookRotation(targetDir);
        
        Vector3 targetRotEuler = targetDirRot.eulerAngles;
        
        if (returnsToStartRot)
        {
            targetRotEuler = ClampEulerRot(WorldStartingRot().eulerAngles, targetRotEuler, viewRadius / 2,
                false);
        }
        else 
        {
            targetRotEuler = ClampEulerRot(WorldStartingRot().eulerAngles, targetRotEuler, viewRadius / 2,
                true, currentRotation);
        }
        
        targetRotation = Quaternion.Euler(targetRotEuler);
        
        BarrelBase.transform.rotation = Quaternion.Slerp(BarrelBase.transform.rotation, targetRotation, lookSpeed);
        
        currentRotation = BarrelBase.transform.rotation.eulerAngles;
        
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
    
    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            playerpos = newTarget;
        }
        else
        {
            Debug.LogWarning("New target is null. Cannot set target.");
        }
    }
}