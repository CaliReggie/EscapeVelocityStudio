using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum EasingType // Changes step animation behaviourTODO: SHOW!!!!
    {
        None,
        EaseIn,
        EaseOut,
        EaseInOut
    }

public class LegMovement : MonoBehaviour
{
    [Header("Prediction References")]
    
    [SerializeField] private Transform legTargetLoc; // Transform that rig guides tip to
    
    [field: SerializeField] public Transform LegRealLoc { get; private set; } // Represents tip of the leg
    
    [Tooltip("The transform that will be used to predict the leg point")]
    [field: SerializeField] private Transform stepPredictionOrigin; // Location used to get ground hit
    
    
    [Header("Prediction Settings")]

    [Tooltip("From the highest point on the leg, what is the real distance to the ground")]
    [SerializeField] private float legHeight = 25f;
    
    [SerializeField] private float predictionRadius = 0.5f; //Spherecast radius
    
    [SerializeField] private LayerMask detectionLayers;

    [Header("Behaviour References")] // TODO: SHOW!!!!

    [Tooltip("Legs references here will be called to step when this one does")]
    [SerializeField] private LegMovement[] snychrononousLegs;
     
    [Tooltip("Will not step while these legs are stepping")]
    [SerializeField] private LegMovement[] asynchronousLegs;
    
    [Header("Behaviour Settings")] // TODO: SHOW!!!!

    [Tooltip("Exists for serialization purposes")]
    [SerializeField] private bool emptyBool;

    [field: SerializeField] public float StepDistance { get; set; } = 1f; //At distance exceeded will try step
    
    [field: SerializeField] public Vector2 DynamicDistanceRange { get; set; } = new Vector2(0.5f, 1.5f); //See Step()
    
    [field: SerializeField] public Vector2 DynamicDurationMinMax { get; set; } = new Vector2(0.5f, 1f); //See Step()
    
    [Range(0,1)] [field: SerializeField] public float StepHeightFactor { get; set; } = 0.5f; // See Step()
    
    [field: SerializeField] public EasingType StepEasingType { get; set; } = EasingType.None;
    
    [Range(1,10)] [field: SerializeField] public int StepEasingMagnitude { get; set; } = 1; //Affects curve of easing
    
    //Private or non serialized below
    
    private Vector3 _currentTargetPoint; //Point we use to calculate where to put target loc
    
    private Vector3 _previousTargetPoint; //Used to track for debug or error handling

    private bool _detectionMissed; //Used to track if we are missing the ground
    
    private bool Stepping { get; set; } = false; // Read by other legs and Read/Set by this leg
    
    private bool WaitForAsynchronousLegs // False when asyn legs are stepping TODO: SHOW!!!!
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
    
    private bool CanStep // Condensed logic to know when a step can be taken 
    {
        get
        {
            if (Stepping) return false;
            
            if (WaitForAsynchronousLegs) return false;
            
            return ExceedingStepDistance; // Have exceeded distance?, therefore should step
        }
    }
    
    private float CurrentStepDistance => Vector3.Distance(LegRealLoc.position, _currentTargetPoint);
    
    private bool ExceedingStepDistance => CurrentStepDistance > StepDistance;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        _currentTargetPoint = LegRealLoc.position;
        
        _previousTargetPoint = LegRealLoc.position;
        
        ManageDetection();
        
        StopAllCoroutines();
        
        StartCoroutine(Step());
    }
    
    private void Update()
    {
        ManageDetection();
        
        SetTargetLoc();
    }
    
    private void ManageDetection()
    {
        RaycastHit hit;
        
        Vector3 predictionPosition = stepPredictionOrigin.position;
        
        if (Physics.SphereCast(predictionPosition, predictionRadius, Vector3.down, out hit,
                legHeight * 2f, detectionLayers)) //Height * 2 should be good
        {
            _currentTargetPoint = hit.point;
            
            _detectionMissed = false;
        }
        else
        {
            
            // _currentTargetPoint = _previousTargetPoint;
            
            _currentTargetPoint = predictionPosition + Vector3.down * legHeight; //Setting to default
            
            _detectionMissed = true;
        }
        
        _previousTargetPoint = _currentTargetPoint;
    }

    private void SetTargetLoc()
    {
        if (_detectionMissed)
        {
            legTargetLoc.position = _currentTargetPoint; // Will have been calculated in ManageDetection()
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
        
        foreach (LegMovement leg in snychrononousLegs) // Try to make others step
        {
            leg.TryStep();
        }
        
        Vector3 startPos = LegRealLoc.position; //Starting from real leg log for visuals
        
        Vector3 endPos = _currentTargetPoint; //Will be calculated to something we can use
        
        // float ydifference = Mathf.Abs(startPos.y - endPos.y); //TODO maybe care about dif amounts another time
        
        Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
        
        // additional step height is a factor from 0-1 considering how tall the leg is plus the midpoint
        float peakTargetHeight = StepHeightFactor * legHeight;
        
        midPoint.y += peakTargetHeight;
        
        //Using a bezier curve between array of Vector3s. Will update the last point for guaranteed hit
        Vector3[] easingPoints = { startPos, midPoint, endPos };
        
        // Uses distance of step withing dynamic range to act as t for lerp between min and max step speeds
        float stepDuration = Mathf.Lerp(DynamicDurationMinMax.x, DynamicDurationMinMax.y,
            (CurrentStepDistance - DynamicDistanceRange.x) / (DynamicDistanceRange.y - DynamicDistanceRange.x));
        
        float timeLeft = stepDuration;

        // u used as t for bezier curve, also used to track time left
        float u = 0f;
        
        while (u < 1f) // TODO: SHOW!!!! UTILS
        {
            if (_detectionMissed)
            {
                break;
            }
            
            timeLeft -= Time.deltaTime;
            
            easingPoints[2] = _currentTargetPoint; //updating the last point
            
            u = 1 - (timeLeft / stepDuration); // updating u
            
            u = Mathf.Clamp01(u); // guaranteed to end at 1
            
            float stepEasing = 0f; // base declaration

            switch (StepEasingType)
            {
                case EasingType.EaseIn:
                    
                    stepEasing = Mathf.Pow(u, StepEasingMagnitude); //Formula: u^n
                    break;
                
                case EasingType.EaseOut:
                    
                    stepEasing = 1 - Mathf.Pow(1 - u, StepEasingMagnitude); //Formula: 1 - (1-u)^n
                    break;
                
                case EasingType.EaseInOut:
                    
                    stepEasing = u < 0.5f ?                                         // Formula: u < 0.5 ?
                        Mathf.Pow(u * 2, StepEasingMagnitude) / 2 :            // (u*2)^n/2 :
                        1 - (Mathf.Pow((1 - u) * 2, StepEasingMagnitude) / 2); // 1 - ((1-u)*2)^n/2
                    break;
                
                case EasingType.None:
                    
                    stepEasing = u; // u already 0-1
                    
                    break;
                
            }
            
            legTargetLoc.position = Utils.Bezier(stepEasing, easingPoints); // See Utils for Bezier
            
            yield return new WaitForEndOfFrame(); // Neat way to run on delta time ?
        }
        
        Stepping = false;
    }
    
    //Debug drawing
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        
        Gizmos.DrawSphere(legTargetLoc.position, 0.5f);
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(stepPredictionOrigin.position, stepPredictionOrigin.position + Vector3.down * 
            legHeight * 2f);
        
        Gizmos.DrawSphere(_currentTargetPoint, 0.5f);
    }
    
    /// <summary>
    /// Attempts to step the leg.
    /// </summary>
    public void TryStep()
    {
        if (!CanStep) return;
        
        StartCoroutine(Step());
    }
}
