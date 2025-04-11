using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LegMovement : MonoBehaviour
{
    [Header("Prediction References")]
    
    [Tooltip("The transform that will represent the target of the leg point")]
    [SerializeField] private Transform legTargetLoc;
    
    [Tooltip("The transform that will be used to predict the leg point")]
    [field: SerializeField] private Transform predictionOrigin;
    
    [Tooltip("The transform that is the tip of the leg")]
    [field: SerializeField] public Transform LegLoc { get; private set; }
    
    [Header("Prediction Settings")]

    [Tooltip("The distance at which the leg place point will be predicted")]
    [SerializeField] private float predictionDistance = 10f;
    
    [SerializeField] private float predictionRadius = 0.5f;
    
    [SerializeField] private LayerMask detectionLayers;
    
    
    private enum EasingType // TODO: SHOW!!!!
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    [Header("Behaviour References")] // TODO: SHOW!!!!

    [SerializeField] private LegMovement[] snychrononousLegs;
    
    [SerializeField] private LegMovement[] asynchronousLegs;
    
    [Header("Behaviour Settings")] // TODO: SHOW!!!!
    
    [SerializeField] private float distanceDifToStep = 5f;
    
    [SerializeField] public float stepDuration = 1f;
    
    [SerializeField] private float peakTargetHeight = 1f;
    
    [SerializeField] private EasingType stepEasingType;
    
    [Range(1,10)] [SerializeField] private int easingMagnitude = 1;
    
    //Private or non serialized below
    
    private Vector3 _currentTargetPoint;
    
    private Vector3 _previousTargetPoint;

    private bool _detectionMissed;
    
    [field: SerializeField] private bool Stepping { get; set; } = false;
    
    private bool WaitForAsynchronousLegs // TODO: SHOW!!!!
    {
        get
        {
            foreach (LegMovement leg in asynchronousLegs)
            {
                if (leg.Stepping) return true;
            }

            return false;
        }
    }
    
    private bool CanStep
    {
        get
        {
            if (Stepping) return false;
            
            if (WaitForAsynchronousLegs) return false;
            
            return ExceedingStepDistance;
        }
    }
    
    private float CurrentStepDistance => Vector3.Distance(LegLoc.position, _currentTargetPoint);
    
    private bool ExceedingStepDistance => CurrentStepDistance > distanceDifToStep;
    
    internal void Initialize()
    {
        _currentTargetPoint = LegLoc.position;
        
        _previousTargetPoint = LegLoc.position;
        
        ManageDetection();
        
        StopAllCoroutines();
        
        StartCoroutine(Step());
    }
    
    public void SetBehaviour( float distance = 0, float duration = 0, float height = 0)
    {
        if (distance > 0) distanceDifToStep = distance;
        
        if (duration > 0) stepDuration = duration;
        
        if (height > 0) peakTargetHeight = height;
    }
    
    private void Update()
    {
        ManageDetection();
        
        SetTargetLoc();
    }
    
    private void ManageDetection()
    {
        RaycastHit hit;
        
        Vector3 predictionPosition = predictionOrigin.position;
        
        if (Physics.SphereCast(predictionPosition, predictionRadius, Vector3.down, out hit,
                predictionDistance, detectionLayers))
        {
            _currentTargetPoint = hit.point;
            
            _detectionMissed = false;
        }
        else
        {
            _currentTargetPoint = _previousTargetPoint;
            
            _detectionMissed = true;
        }
        
        _previousTargetPoint = _currentTargetPoint;
    }

    private void SetTargetLoc()
    {
        if (_detectionMissed)
        {
            legTargetLoc.position = _currentTargetPoint;
        }
        else
        {
            if (CanStep)
            {
                StartCoroutine(Step());
            }
        }
    }
    
    private IEnumerator Step() // TODO: SHOW!!!!
    {
        Stepping = true;
        
        foreach (LegMovement leg in snychrononousLegs)
        {
            leg.TryStep();
        }
        
        Vector3 startPos = LegLoc.position;
        
        Vector3 endPos = _currentTargetPoint;
        
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        
        // float ydifference = Mathf.Abs(startPos.y - endPos.y); //TODO maybe care about dif amounts another time
        
        midPoint.y += peakTargetHeight;
        
        Vector3[] easingPoints = new Vector3[3];
        
        easingPoints[0] = startPos;
        
        easingPoints[1] = midPoint;
        
        easingPoints[2] = endPos;
        
        float timeLeft = stepDuration;

        float u = 0f;
        
        while (u < 1f) // TODO: SHOW!!!! UTILS
        {
            if (_detectionMissed)
            {
                break;
            }
            
            timeLeft -= Time.deltaTime;
            
            easingPoints[2] = _currentTargetPoint;
            
            u = 1 - (timeLeft / stepDuration);
            
            u = Mathf.Clamp01(u);
            
            float stepEasing = 0f;

            switch (stepEasingType)
            {
                case EasingType.EaseIn:
                    
                    stepEasing = Mathf.Pow(u, easingMagnitude);
                    break;
                
                case EasingType.EaseOut:
                    
                    stepEasing = 1 - Mathf.Pow(1 - u, easingMagnitude);
                    break;
                
                case EasingType.EaseInOut:
                    
                    stepEasing = u < 0.5f ? 
                        Mathf.Pow(u * 2, easingMagnitude) / 2 : 
                        1 - (Mathf.Pow((1 - u) * 2, easingMagnitude) / 2);
                    break;
                
                case EasingType.None:
                    
                    stepEasing = u;
                    break;
                
            }
            
            legTargetLoc.position = Utils.Bezier(stepEasing, easingPoints);
            
            yield return new WaitForEndOfFrame();
        }
        
        Stepping = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        
        Gizmos.DrawSphere(legTargetLoc.position, 0.5f);
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(predictionOrigin.position, predictionOrigin.position + Vector3.down * 
            predictionDistance);
        
        Gizmos.DrawSphere(_currentTargetPoint, 0.5f);
    }
    
    private void TryStep()
    {
        if (!CanStep) return;
        
        StartCoroutine(Step());
    }
}
