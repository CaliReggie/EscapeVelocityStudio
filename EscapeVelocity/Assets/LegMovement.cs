using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LegMovement : MonoBehaviour
{
    [Header("Prediction References")]
    
    [Tooltip("The transform that will be used to predict the leg point")]
    [SerializeField] private Transform predictionOrigin;
    
    [Tooltip("The transform that will represent the target of the leg point")]
    [SerializeField] private Transform legTargetLoc;
    
    [Tooltip("The transform that is the tip of the leg")]
    [SerializeField] private Transform legLoc;
    
    [Header("Prediction Settings")]

    [Tooltip("The distance at which the leg place point will be predicted")]
    [SerializeField] private float predictionDistance = 10f;
    
    [SerializeField] private float predictionRadius = 0.5f;
    
    [SerializeField] private LayerMask detectionLayers;
    
    [Range(0,1)] [SerializeField] private float missEasing = 0.5f; //TODO: TS sucks rn
    
    [SerializeField] private bool constrainRange;
    
    [SerializeField] private Vector2 constrainedRange = new (1f, 10f);
    
    
    private enum EasingType
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    [Header("Behaviour References")]

    [SerializeField] private LegMovement[] snychrononousLegs;
    
    [SerializeField] private LegMovement[] asynchronousLegs;
    
    [Header("Behaviour Settings")]
    
    [SerializeField] private float distanceDifToStep = 5f;
    
    [SerializeField] private float stepDuration = 1f;
    
    [SerializeField] private float peakTargetHeight = 1f;
    
    [SerializeField] private EasingType stepEasingType;
    
    [Range(1,10)] [SerializeField] private int easingMagnitude = 1;
    
    //Private or non serialized below
    
    private Vector3 _currentTargetPoint;
    
    private Vector3 _previousTargetPoint;

    private bool _detectionMissed;
    
    public bool Stepping { get; private set; } = false;
    
    private bool ShouldMatchSynchronousLegs
    {
        get
        {
            foreach (LegMovement leg in snychrononousLegs)
            {
                if (leg.Stepping) return true;
            }

            return false;
        }
    }
    
    private bool ShouldWaitAsynchronousLegs
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
    
    internal void Initialize()
    {
        _currentTargetPoint = legLoc.position;
        
        _previousTargetPoint = legLoc.position;

        StopAllCoroutines();
        
        StartCoroutine(Step());
    }
    
    private void Update()
    {
        ManageDetection();
        
        ManageTargetPoint();
        
        SetTargetLoc();
    }
    
    private void ManageDetection()
    {
        RaycastHit hit;
        
        if (Physics.SphereCast(predictionOrigin.position, predictionRadius, Vector3.down, out hit,
                predictionDistance, detectionLayers))
        {
            _currentTargetPoint = hit.point;
            
            _detectionMissed = false;
        }
        else
        {
            _currentTargetPoint = Vector3.Lerp(_previousTargetPoint, legLoc.position, missEasing);
            
            _detectionMissed = true;
        }
    }
    
    private void ManageTargetPoint()
    {
        
        if (constrainRange)
        {
            Vector3 targetDirection = _currentTargetPoint - predictionOrigin.position;
            
            float targetDistance = targetDirection.magnitude;
            
            if (targetDistance < constrainedRange.x)
            {
                _currentTargetPoint = predictionOrigin.position + targetDirection.normalized * constrainedRange.x;
            }
            else if (targetDistance > constrainedRange.y)
            {
                _currentTargetPoint = predictionOrigin.position + targetDirection.normalized * constrainedRange.y;
            }
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
            // if (Stepping) return;
            //
            // if (LegDistanceToTarget > distanceDifToStep)
            // {
            //     StartCoroutine(Step());
            // }
            
            if (CanStep)
            {
                StartCoroutine(Step());
            }
        }
    }
    
    private IEnumerator Step()
    {
        Stepping = true;
        
        Vector3 startPos = legLoc.position;
        
        Vector3 endPos = _currentTargetPoint;
        
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        
        // float ydifference = Mathf.Abs(startPos.y - endPos.y); //TODO maybe care about dif amounts another time
        
        midPoint.y += peakTargetHeight;
        
        Vector3[] easingPoints = new Vector3[3];
        
        easingPoints[0] = startPos;
        
        easingPoints[1] = midPoint;
        
        easingPoints[2] = endPos;
        
        float timeLeft = stepDuration;
        
        while (LegDistanceToTarget > 1f)
        {
            if (_detectionMissed)
            {
                Stepping = false;
                
                break;
            }
            
            easingPoints[2] = _currentTargetPoint;
            
            float u = 1 - (timeLeft / stepDuration);
            
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
            
            timeLeft -= Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }
        
        if (!Stepping) { yield break; }
        
        legTargetLoc.position = _currentTargetPoint;
        
        Stepping = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(predictionOrigin.position, predictionOrigin.position + Vector3.down * 
            predictionDistance);
        
        Gizmos.DrawSphere(_currentTargetPoint, 0.5f);
    }
    
    
    
    private bool CanStep
    {
        get
        {
            if (Stepping) return false;
            
            if (ShouldMatchSynchronousLegs) return true;
            
            if (ShouldWaitAsynchronousLegs) return false;

            // return Vector3.Distance(legLoc.position, _currentTargetPoint) > distanceDifToStep; ?
            
            return LegDistanceToTarget > distanceDifToStep;
        }
    }
    
    private float LegDistanceToTarget => Vector3.Distance(legTargetLoc.position, _currentTargetPoint);
}
