using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyMovement : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private LegReference[] legs;

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
    
    private Vector3 _curPositionFromLegs;
    
    private Quaternion _curRotationFromLegs;
    
    private Quaternion _rotationOffsetFromInput = Quaternion.identity;
    
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
        
        _curPositionFromLegs = AverageLegPosition + transform.up * targetBodyHeight + transform.right * lateralOffset;
        
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
        
        _curPositionFromLegs = AverageLegPosition + transform.up * targetBodyHeight + transform.right * lateralOffset;
        
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
        Vector3 averageFront = (_averageFrontLeftPosition + _averageFrontRightPosition) / 2f;
        Vector3 averageBack = (_averageBackLeftPosition + _averageBackRightPosition) / 2f;
        Vector3 averageLeft = (_averageBackLeftPosition + _averageFrontLeftPosition) / 2f;
        Vector3 averageRight = (_averageBackRightPosition + _averageFrontRightPosition) / 2f;

        Vector3 forward = (averageFront - averageBack).normalized;
        Vector3 right = (averageRight - averageLeft).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        // Re-orthogonalize right in case of non-perfect leg placement
        right = Vector3.Cross(up, forward).normalized;

        // Build rotation from direction vectors
        _curRotationFromLegs = Quaternion.LookRotation(forward, up);

    }
    
    private void BodyMove()
    {
        Vector3 targetBodyPos = transform.position;
        
        targetBodyPos.y = _curPositionFromLegs.y;
        
        if (move) { targetBodyPos += moveSpeed * Time.deltaTime * transform.forward; }
        
        transform.position = Vector3.Lerp(transform.position, targetBodyPos, posEasing);
    }
    
    private void BodyRotate()
    {
        Quaternion targetRotation = _curRotationFromLegs;
        
        if (rotate) //If rotating, we move from current rotation to added rotation from settings
        {
            targetRotation *= Quaternion.Euler(rotationSpeed * Time.deltaTime * rotationAddition);
        } 
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotEasing);
    }
    
    private void OnDrawGizmosSelected()
    {
        //return if not playing
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.red;
        
        Gizmos.DrawSphere(AverageLegPosition, 0.5f);
        
        Gizmos.color = Color.green;
        
        Gizmos.DrawSphere(_curPositionFromLegs, 0.5f);

        Gizmos.DrawRay(AverageLegPosition, _curRotationFromLegs * Vector3.forward * 5f);
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
