using System;
using UnityEngine;
using UnityEngine.Serialization;

public class LegMovement : MonoBehaviour
{
    [Header("Leg Point Prediction")]
    
    [Tooltip("The transform that will be used to predict the leg point")]
    [SerializeField] private Transform predictionDirection;
    
    [Tooltip("The transform that will represent the leg tip place point")]
    [SerializeField] private Transform predictionPoint;
    
    [Tooltip("The transform that is the tip of the leg")]
    [SerializeField] private Transform currentPoint;

    [Tooltip("The distance at which the leg place point will be predicted")]
    [SerializeField] private float predictionDistance = 10f;
    
    [SerializeField] private LayerMask detectionLayers;
    
    public bool stopUpdate = false;
    
    
    private Vector3 lastPosition;

    // [Header("Leg Movement")]
    //
    // [Tooltip("The distance at which the leg will be picked up and moved")]
    // [SerializeField] private float stepOffset = 10f;
    //
    // [Tooltip("The speed at which the leg will be moved")]
    // [SerializeField] private float stepSpeed = 5f;

    private void Update()
    {
        
        if (stopUpdate)
        {
            predictionPoint.position = lastPosition;
            
            return;
        }
        // Raycast to find the ground
        RaycastHit hit;
        if (Physics.Raycast(predictionDirection.position, predictionDirection.forward, out hit, predictionDistance, detectionLayers))
        {
            predictionPoint.position = hit.point;
        }
        else
        {
            predictionPoint.position = predictionDirection.position + predictionDirection.forward * predictionDistance;
        }
        
        lastPosition = predictionPoint.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(predictionDirection.position, predictionDirection.position + predictionDirection.forward * 
            predictionDistance);
    }
}
