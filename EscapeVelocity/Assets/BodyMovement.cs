using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyMovement : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private LegReference[] legs;

    [SerializeField]
    private Transform movementOrientation;

    [Header("Body Movement")]
    
    [SerializeField] private float targetBodyHeight = 1f;
    
    [SerializeField] private float lateralOffset = 0f;
    
    [Range(0,1)] [SerializeField] private float posEasing = 0.5f;
    
    [SerializeField] bool move;

    [SerializeField] private float moveSpeed = 1f;
    
    [SerializeField] private bool rotate;
    
    [SerializeField] private Vector3 rotationAddition;

    [SerializeField]
    private float rotationSpeed;
    
    [Range(0,1)] [SerializeField] private float rotEasing = 0.5f;
    
    [Header("Leg Behaviour")]
    
    [SerializeField] private bool overrideLegBehaviour;
    
    [SerializeField] private float stepDistance = 1f;
    
    [SerializeField] private float stepDuration = 1f;
    
    [SerializeField] private float stepHeight = 1f;
    
    //Private, or non serialized below
    
    private Vector3 _averageBackLeftPosition;
    
    private Vector3 _averageFrontLeftPosition;
    
    private Vector3 _averageFrontRightPosition;
    
    private Vector3 _averageBackRightPosition;
    
    int BLCount, FLCount, FRCount, BRCount;
    
    private Vector3 _curTargetPosition;
    
    private Quaternion _curTargetRotation;
    
    private bool _legsInitialized;
    
    private Vector3 AverageLegPosition
    {
        get
        {
            Vector3 averageLegPosition = _averageBackLeftPosition +
                                         _averageFrontLeftPosition +
                                         _averageFrontRightPosition +
                                         _averageBackRightPosition;
            
            averageLegPosition /= 4f;
            
            return averageLegPosition;
        }
    }

    private void OnValidate()
    {
        if (overrideLegBehaviour)
        {
            foreach (LegReference leg in legs)
            {
                leg.legMovement.SetBehaviour(stepDistance, stepDuration, stepHeight);
            }
            
            overrideLegBehaviour = false;
        }
    }


    private void OnEnable()
    {
        StartCoroutine(InitLegs());
    }

    private IEnumerator InitLegs()
    {
        _averageBackLeftPosition = Vector3.zero;
        _averageFrontLeftPosition = Vector3.zero;
        _averageFrontRightPosition = Vector3.zero;
        _averageBackRightPosition = Vector3.zero;
        
        BLCount = 0;
        FLCount = 0;
        FRCount = 0;
        BRCount = 0;
        
        yield return new WaitForSeconds(0.1f);
        
        foreach (LegReference leg in legs)
        {
            leg.legMovement.Initialize();
            
            leg.legMovement.SetBehaviour(stepDistance, stepDuration, stepHeight);
            
            if (leg.legType == LegType.BackLeft)
            {
                _averageBackLeftPosition += leg.legMovement.LegLoc.position;
                
                BLCount++;
            }
            else if (leg.legType == LegType.FrontLeft)
            {
                _averageFrontLeftPosition += leg.legMovement.LegLoc.position;
                
                FLCount++;
            }
            else if (leg.legType == LegType.FrontRight)
            {
                _averageFrontRightPosition += leg.legMovement.LegLoc.position;
                
                FRCount++;
            }
            else if (leg.legType == LegType.BackRight)
            {
                _averageBackRightPosition += leg.legMovement.LegLoc.position;
                
                BRCount++;
            }
        }
        
        _averageBackLeftPosition /= BLCount;
        
        _averageFrontLeftPosition /= FLCount;
        
        _averageFrontRightPosition /= FRCount;
        
        _averageBackRightPosition /= BRCount;
        
        _curTargetPosition = AverageLegPosition + transform.up * targetBodyHeight + transform.right * lateralOffset;
        
        _legsInitialized = true;
    }

    private void Update()
    {
        if (!_legsInitialized) return;
        
        CalculatePosition();
        
        BodyMove();
        
        CalculateRotation();

        BodyRotate();
    }

    private void CalculatePosition()
    {
        RefreshAverages();
        
        foreach (LegReference leg in legs)
        {
            switch (leg.legType)
            {
                case LegType.BackLeft:
                    _averageBackLeftPosition += leg.legMovement.LegLoc.position;
                    break;
                case LegType.FrontLeft:
                    _averageFrontLeftPosition += leg.legMovement.LegLoc.position;
                    break;
                case LegType.FrontRight:
                    _averageFrontRightPosition += leg.legMovement.LegLoc.position;
                    break;
                case LegType.BackRight:
                    _averageBackRightPosition += leg.legMovement.LegLoc.position;
                    break;
            }
        }
        
        _averageBackLeftPosition /= BLCount;
        
        _averageFrontLeftPosition /= FLCount;
        
        _averageFrontRightPosition /= FRCount;
        
        _averageBackRightPosition /= BRCount;
        
        _curTargetPosition = AverageLegPosition + transform.up * targetBodyHeight + transform.right * lateralOffset;
        
        return;
        
        void RefreshAverages()
        {
            _averageBackLeftPosition = Vector3.zero;
            _averageFrontLeftPosition = Vector3.zero;
            _averageFrontRightPosition = Vector3.zero;
            _averageBackRightPosition = Vector3.zero;
        }
    }
    
    private void CalculateRotation()
    {
        float pitch; //Determined by averages of fronts vs back legs
        float yaw; //Determined by averages of lefts vs rights
        
        Vector3 averageFront = (_averageFrontLeftPosition + _averageFrontRightPosition) / 2f;
        Vector3 averageBack = (_averageBackLeftPosition + _averageBackRightPosition) / 2f;
        Vector3 averageLeft = (_averageBackLeftPosition + _averageFrontLeftPosition) / 2f;
        Vector3 averageRight = (_averageBackRightPosition + _averageFrontRightPosition) / 2f;
        
        Vector3 averageFrontBack = averageFront - averageBack;
        
        Vector3 averageLeftRight = averageLeft - averageRight;
        
        pitch = Mathf.Atan2(averageFrontBack.y, averageFrontBack.magnitude) * Mathf.Rad2Deg;
        
        yaw = Mathf.Atan2(averageLeftRight.y, averageLeftRight.magnitude) * Mathf.Rad2Deg;
        
        _curTargetRotation = Quaternion.Euler(-pitch, -yaw, 0f);
        
    }
    
    private void BodyMove()
    {
        Vector3 targetBodyPos = transform.position;
        
        targetBodyPos.y = _curTargetPosition.y;
        
        if (move) { targetBodyPos += moveSpeed * Time.deltaTime * transform.forward; }
        
        transform.position = Vector3.Lerp(transform.position, targetBodyPos, posEasing);
        
        movementOrientation.position = transform.position;
    }
    
    private void BodyRotate()
    {
        Quaternion targetRotation = _curTargetRotation;
        
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotEasing);
        transform.rotation = _curTargetRotation;
        
        if (rotate)
        {
           Quaternion targetOrientationRotation = transform.rotation * Quaternion.Euler(rotationSpeed * Time.deltaTime * rotationAddition);

           movementOrientation.rotation = targetOrientationRotation;
            
            Debug.DrawRay(movementOrientation.position, movementOrientation.forward * 5f, Color.red);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        //return if not playing
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawSphere(AverageLegPosition, 0.5f);
        
        Gizmos.color = Color.green;
        
        Gizmos.DrawSphere(_curTargetPosition, 0.5f);

        Gizmos.DrawRay(AverageLegPosition, _curTargetRotation * Vector3.forward * 5f);
    }
}

public enum LegType
{
    None,
    BackLeft,
    FrontLeft,
    FrontRight,
    BackRight
}

[Serializable]
public class LegReference
{
    public LegMovement legMovement;
    
    public LegType legType;
}
