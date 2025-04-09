using System;
using System.Collections;
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
    
    private enum MissBehaviour
    {
        KeepLastLoc,
        SetPredictionOrigin,
        SetMaxPrediction,
        SetMinConstrained,
        SetMaxConstrained
    }

    [SerializeField] private MissBehaviour missBehaviour;
    
    
    
    [Header("Behaviour")]
    
    [Range(0,1)] [SerializeField] private float stepEasing = 0.5f;

    [SerializeField] private float distanceToStep = 10f;
    
    [SerializeField] private bool constrainRange;
    
    [SerializeField] private Vector2 constrainedRange = new (1f, 10f);
    
    [Header("Dynamic")]
    
    [SerializeField] private bool stepping = false;
    
    //Private or non serialized below
    
    private Vector3 _currentTargetPoint;
    
    private Vector3 _lastTargetPoint;
    
    private void Update()
    {
        ManageDetection();
        
        ManageTargetPoint();
        
        SetTargetLoc();
    }
    
    private void ManageDetection()
    {
        RaycastHit hit;
        
        if (Physics.SphereCast(predictionOrigin.position, predictionRadius, predictionOrigin.forward, out hit,
                predictionDistance, detectionLayers))
        {
            _currentTargetPoint = hit.point;
        }
        else
        {
            switch (missBehaviour)
            {
                case MissBehaviour.KeepLastLoc:
                    
                    _currentTargetPoint = _lastTargetPoint;
                    break;
                
                case MissBehaviour.SetPredictionOrigin:
                    
                    _currentTargetPoint = predictionOrigin.position;
                    break;
                
                case MissBehaviour.SetMaxPrediction:
                    
                    _currentTargetPoint = predictionOrigin.position + predictionOrigin.forward * predictionDistance;
                    break;
                    
                case MissBehaviour.SetMinConstrained:
                    
                    _currentTargetPoint = predictionOrigin.position + predictionOrigin.forward * constrainedRange.x;
                    break;
                
                case MissBehaviour.SetMaxConstrained:
                    
                    _currentTargetPoint = predictionOrigin.position + predictionOrigin.forward * constrainedRange.y;
                    break;
            }
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
        
        _lastTargetPoint = _currentTargetPoint;
    }

    private void SetTargetLoc()
    {
        if (stepping) return;
        
        if (LegDistanceToTarget > distanceToStep)
        {
            StartCoroutine(Step());
        }
    }
    
    private IEnumerator Step()
    {
        stepping = true;
        
        while (LegDistanceToTarget > 1f)
        {
            legTargetLoc.position = Vector3.Lerp(legTargetLoc.position, _currentTargetPoint, stepEasing);
            
            yield return new WaitForEndOfFrame();
        }
        
        legTargetLoc.position = _currentTargetPoint;
        
        stepping = false;
    }
    
    private float LegDistanceToTarget => Vector3.Distance(legTargetLoc.position, _currentTargetPoint);


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(predictionOrigin.position, predictionOrigin.position + predictionOrigin.forward * 
            predictionDistance);
    }
    
}
