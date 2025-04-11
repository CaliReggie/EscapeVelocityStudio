using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyMovement : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private Boss boss; // Used to query for information on behaviour
    
    [SerializeField] private LegReference[] legs; // Wrapper list for managing attached legs

    [Header("Body Movement")]
    
    [SerializeField] private float targetBodyHeight = 1f; // Height above avg leg position to move body to

    [SerializeField] private float moveSpeed = 1f; //Speed at which lateral target move is set
    
    [Range(0,1)] [SerializeField] private float posEasing = 0.5f; //Easing between current and target position
    
    [SerializeField] private float rotationSpeed = 60; //Speed at which body rotates, kinda jank rn
    
    [Range(0,1)] [SerializeField] private float rotEasing = 0.5f; //Easing between current and target rotation
    
    [Header("Leg Behaviour")]
    
    [Tooltip("Click to assign below settings to all attached legs")]
    [SerializeField] private bool setAllLegSettings;
    
    [SerializeField] private float stepDistance = 1f; //See LegMovement
    
    [SerializeField] private Vector2 dynamicDistanceRange = new Vector2(0.5f, 1.5f);  //See LegMovement
    
    [SerializeField] private Vector2 dynamicDurationMinMax = new Vector2(0.5f, 1f); //See LegMovement
    
    [Range(0,1)] [SerializeField] private float stepHeightFactor = 0.5f; // See LegMovement
    
    [SerializeField] private EasingType stepEasingType = EasingType.None; // See LegMovement
    
    [Range(1,10)] [SerializeField] private int stepEasingMagnitude = 1; //See LegMovement
    
    //Private, or non serialized below
    
    //Input
    private Vector2 _moveInput; //Lateral movement input, set from boss query or (deprecated) user input
    
    private float _rotationInput; //Rotation input, set from boss query or (deprecated) user input
                                  //(-1 for left, 1 for right)
                                  
    private Vector3 _averageUnweightedPosition; //Used for weighting body position and rotation
    
    private Vector3 _averageBackLeftPosition; //Used for weighting body position and rotation
    
    private Vector3 _averageFrontLeftPosition; //Used for weighting body position and rotation
    
    private Vector3 _averageFrontRightPosition; //Used for weighting body position and rotation
    
    private Vector3 _averageBackRightPosition; //Used for weighting body position and rotation
    
    int XCount, BLCount, FLCount, FRCount, BRCount; //Counts of legs of each type, used for averaging
    
    private Vector3 _targetPosAboveLegs; // Current average of all leg positions plus height, used for body position
    
    private Quaternion _curRotationFromLegs; // Current average target rotation from leg positions
    
    private Quaternion _rotationOffsetFromInput = Quaternion.identity; // Used for additive turning
    
    private bool _legsInitialized; // Flag to make sure we don't try to calculate with null values
    
    private Vector3 AverageLegPosition // Averages all types of legs to get a single point
    {
        get
        {
            Vector3[] legPositions = {
                _averageUnweightedPosition,
                _averageBackLeftPosition,
                _averageFrontLeftPosition,
                _averageFrontRightPosition,
                _averageBackRightPosition
            };
            
            Vector3 averageLegPosition = Vector3.zero;
            
            for (int i = 0; i < legPositions.Length; i++)
            {
                if (legPositions[i] == Vector3.zero) continue; // Skip non calculated values
                
                averageLegPosition += legPositions[i];
            }

            averageLegPosition /= legPositions.Length;
            
            return averageLegPosition;
        }
    }

    private void OnValidate() //Way to set values in editor regardless of playmode or not
    {
        if (setAllLegSettings)
        {
            SetLegSettings();
            
            setAllLegSettings = false;
        }
    }
    
    private void SetLegSettings()
    {
        foreach (LegReference leg in legs)
        {
            leg.legMovement.StepDistance = stepDistance;
            
            leg.legMovement.DynamicDistanceRange = dynamicDistanceRange;
            
            leg.legMovement.DynamicDurationMinMax = dynamicDurationMinMax;
            
            leg.legMovement.StepHeightFactor = stepHeightFactor;
            
            leg.legMovement.StepEasingType = stepEasingType;
            
            leg.legMovement.StepEasingMagnitude = stepEasingMagnitude;
        }
    }

    private void OnEnable()
    {
        InitLegs();
    }
    
    private void InitLegs()
    {
        RefreshCounts();
        
        RefreshAverages();
        
        _targetPosAboveLegs = AverageLegPosition + transform.up * targetBodyHeight;
        
        _legsInitialized = true;
    }

    private void Update()
    {
        if (!_legsInitialized) return;
        
        if (!boss.HasTarget) return;

        SetMoveInput();
        
        CalculatePosition();
        
        BodyMove();
        
        CalculateRotation();

        BodyRotate();
    }
    
    // private void GetMoveInput()
    // {
    //     _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    //     
    //     if (Input.GetKey(KeyCode.LeftShift))
    //     {
    //         _moveInput *= 3f;
    //     }
    //     
    //     float leftLook = Input.GetKey(KeyCode.Mouse0) ? -1 : 0;
    //     
    //     float rightLook = Input.GetKey(KeyCode.Mouse1) ? 1 : 0;
    //     
    //     _rotationInput = leftLook + rightLook;
    // }
    
    /// <summary>
    /// Query to boss and set a target lateral movement and rotation input based on a target position
    /// </summary>
    private void SetMoveInput()
    {
        Vector3 targetPosition = boss.GetTargetPosition(); // See Boss.cs
        
        Vector3 toTarget = targetPosition - transform.position; // Target direction
        
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget < boss.StoppingDistance) //Nothing if in range
        {
            _moveInput = Vector2.zero;
            _rotationInput = 0f;
            return;
        }
        
        Vector3 localToTarget = transform.InverseTransformDirection(toTarget.normalized); //World to local direction
        
        _moveInput = new Vector2(localToTarget.x, localToTarget.z); // Horizontal part of said direction
        
        _moveInput = Vector2.ClampMagnitude(_moveInput, 1f); // Clamped to replicate input
        
        Vector3 flatToTarget = toTarget;
        flatToTarget.y = 0; // Horizontal direction to target
        
        Vector3 flatForward = transform.forward;
        flatForward.y = 0; // Horizontal forward direction

        Vector3 cross = Vector3.Cross(flatForward.normalized, flatToTarget.normalized); //Cross for orientation
        
        _rotationInput = Mathf.Sign(Vector3.Dot(cross, Vector3.up));  //Sign of the cross product

        float angle                    = Vector3.Angle(flatForward, flatToTarget); //Angle between forward and target
        if (angle < 5f) _rotationInput = 0f; // Ignore small angles
    }

    private void CalculatePosition() // TODO: SHOW!!!!
    {
        RefreshAverages();

        _targetPosAboveLegs = AverageLegPosition + transform.up * targetBodyHeight;
        
        return;
    }
    
    private void RefreshCounts()
    {
        BLCount = 0;
        FLCount = 0;
        FRCount = 0;
        BRCount = 0;
        
        foreach (LegReference leg in legs)
        {
            if (leg.legType == LegType.BackLeft) BLCount++;
            else if (leg.legType == LegType.FrontLeft) FLCount++;
            else if (leg.legType == LegType.FrontRight) FRCount++;
            else if (leg.legType == LegType.BackRight) BRCount++;
        }
    }
    
    private void RefreshAverages()
    {
        _averageUnweightedPosition = Vector3.zero;
        _averageBackLeftPosition = Vector3.zero;
        _averageFrontLeftPosition = Vector3.zero;
        _averageFrontRightPosition = Vector3.zero;
        _averageBackRightPosition = Vector3.zero;
        
        foreach (LegReference leg in legs) //Adding weights by type
        {
            switch (leg.legType)
            {
                case LegType.Unweighted:
                    _averageUnweightedPosition += leg.legMovement.LegRealLoc.position;
                    break;
                case LegType.BackLeft:
                    _averageBackLeftPosition += leg.legMovement.LegRealLoc.position;
                    break;
                case LegType.FrontLeft:
                    _averageFrontLeftPosition += leg.legMovement.LegRealLoc.position;
                    break;
                case LegType.FrontRight:
                    _averageFrontRightPosition += leg.legMovement.LegRealLoc.position;
                    break;
                case LegType.BackRight:
                    _averageBackRightPosition += leg.legMovement.LegRealLoc.position;
                    break;
            }
        }
        
        //Dividing by counts for averages
        
        _averageUnweightedPosition /= XCount;
        
        _averageBackLeftPosition /= BLCount;
        
        _averageFrontLeftPosition /= FLCount;
        
        _averageFrontRightPosition /= FRCount;
        
        _averageBackRightPosition /= BRCount;
    }
    
    private void CalculateRotation() // TODO: SHOW!!!!
    {
        // Calculating weights by body position
        
        Vector3 averageFront = (_averageFrontLeftPosition + _averageFrontRightPosition) / 2f;
        Vector3 averageBack = (_averageBackLeftPosition + _averageBackRightPosition) / 2f;
        Vector3 averageLeft = (_averageBackLeftPosition + _averageFrontLeftPosition) / 2f;
        Vector3 averageRight = (_averageBackRightPosition + _averageFrontRightPosition) / 2f;

        Vector3 forward = (averageFront - averageBack).normalized; // Forward weight direction
        Vector3 right = (averageRight - averageLeft).normalized; // Right weight direction
        Vector3 up = Vector3.Cross(forward, right).normalized;
        
        right = Vector3.Cross(up, forward).normalized; //For future issues with correcting leg placement
        
        _curRotationFromLegs = Quaternion.LookRotation(forward, up); // Rotation from legs
    }
    
    private void BodyMove() // TODO: SHOW!!!!
    {
        Vector3 targetBodyPos = transform.position;
        
        targetBodyPos.y = _targetPosAboveLegs.y; // Target pos is added height of target pos
        
        if (_moveInput != Vector2.zero) //Adding lateral movement
        {
            Vector2 additionalMove = moveSpeed * Time.deltaTime * new Vector2(_moveInput.x, _moveInput.y);
            
            targetBodyPos += transform.forward * additionalMove.y;
            
            targetBodyPos += transform.right * additionalMove.x;
        }
        
        transform.position = Vector3.Lerp(transform.position, targetBodyPos, posEasing); //Lerping to target
    }
    
    private void BodyRotate() // TODO: SHOW!!!!
    {
        if (_rotationInput != 0) // Kinda janky, adding rotation to current target like this to avoid snapping
        {
            Quaternion rotationAddition = Quaternion.Euler(0, _rotationInput * rotationSpeed * Time.deltaTime, 0);
            
            _rotationOffsetFromInput *= rotationAddition;
            
            _rotationOffsetFromInput.y = Mathf.Clamp(_rotationOffsetFromInput.y, -.25f, .25f);
            
            _rotationOffsetFromInput.w = 1f; // Quaternion weirdness
        }
        else //Go back to fully weighted look rotation
        {
            _rotationOffsetFromInput = Quaternion.Slerp(_rotationOffsetFromInput, Quaternion.identity, 0.1f);
        }
        
        Quaternion targetRotation = _curRotationFromLegs * _rotationOffsetFromInput;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotEasing); // Rot lerping
    }
    
    //Drawing debug info
    private void OnDrawGizmosSelected()
    {
        //return if not playing
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.blue;
        
        Gizmos.DrawSphere(AverageLegPosition, 0.5f);
        
        Gizmos.color = Color.green;
        
        Gizmos.DrawSphere(_targetPosAboveLegs, 0.5f);

        Gizmos.DrawRay(AverageLegPosition, _curRotationFromLegs * Vector3.forward * 5f);
    }
}

public enum LegType
{
    Unweighted,
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
