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

    [SerializeField] private float moveSpeed = 1f;
    
    [Range(0,1)] [SerializeField] private float posEasing = 0.5f;
    
    [SerializeField] private float rotationSpeed;
    
    [Range(0,1)] [SerializeField] private float rotEasing = 0.5f;
    
    [Header("Leg Behaviour")]
    
    [SerializeField] private bool overrideLegBehaviour;
    
    [SerializeField] private float stepDistance = 1f;
    
    [SerializeField] private float stepDuration = 1f;
    
    [SerializeField] private float stepHeight = 1f;
    
    //Private, or non serialized below
    
    //Input
    private Vector2 _moveInput;
    
    private float _rotationInput;
    
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

        _curPositionFromLegs = AverageLegPosition + transform.up * targetBodyHeight;
        
        _legsInitialized = true;
    }

    private void Update()
    {
        if (!_legsInitialized) return;

        GetInput();
        
        CalculatePosition();
        
        BodyMove();
        
        CalculateRotation();

        BodyRotate();
    }
    
    private void GetInput()
    {
        _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _moveInput *= 3f;
        }
        
        float leftLook = Input.GetKey(KeyCode.Mouse0) ? -1 : 0;
        
        float rightLook = Input.GetKey(KeyCode.Mouse1) ? 1 : 0;
        
        _rotationInput = leftLook + rightLook;
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

        _curPositionFromLegs = AverageLegPosition + transform.up * targetBodyHeight;
        
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
        
        if (_moveInput != Vector2.zero)
        {
            Vector2 additionalMove = moveSpeed * Time.deltaTime * new Vector2(_moveInput.x, _moveInput.y);
            
            targetBodyPos += transform.forward * additionalMove.y;
            
            targetBodyPos += transform.right * additionalMove.x;
        }
        
        transform.position = Vector3.Lerp(transform.position, targetBodyPos, posEasing);
    }
    
    private void BodyRotate()
    {
        // Accumulate rotation offset if rotating
        if (_rotationInput != 0)
        {
            Quaternion rotationAddition = Quaternion.Euler(0, _rotationInput * rotationSpeed * Time.deltaTime, 0);
            
            _rotationOffsetFromInput *= rotationAddition;
            
            _rotationOffsetFromInput.y = Mathf.Clamp(_rotationOffsetFromInput.y, -.25f, .25f);
            
            _rotationOffsetFromInput.w = 1f;
        }
        else
        {
            // Reset rotation offset if not rotating
            _rotationOffsetFromInput = Quaternion.Slerp(_rotationOffsetFromInput, Quaternion.identity, 0.1f);
        }
        
        Quaternion targetRotation = _curRotationFromLegs * _rotationOffsetFromInput;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotEasing);
    }
    
    private void OnDrawGizmosSelected()
    {
        //return if not playing
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.blue;
        
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
