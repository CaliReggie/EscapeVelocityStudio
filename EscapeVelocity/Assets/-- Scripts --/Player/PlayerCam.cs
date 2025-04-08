using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Serialization;

#if UNITY_EDITOR

//making custom GUI buttons
[CustomEditor(typeof(PlayerCam))]
public class PlayerCamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PlayerCam myScript = (PlayerCam)target;

        if (GUILayout.Button("Switch First Person"))
        {
            myScript.SwitchCamType(ECamType.FirstPerson);
        }

        if (GUILayout.Button("Switch Third Orbit"))
        {
            myScript.SwitchCamType(ECamType.ThirdOrbit);
        }

        if (GUILayout.Button("Switch Third Fixed"))
        {
            myScript.SwitchCamType(ECamType.ThirdFixed);
        }
        
        DrawDefaultInspector();
    }
}

#endif

public enum ECamType
{
    FirstPerson,
    ThirdOrbit,
    ThirdFixed,
}

public enum EControlScheme
    {
        KeyboardAndMouse,
        Gamepad
    }

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCam : MonoBehaviour
{
    
    
    [Header("Cam References")]
    
    [SerializeField] private CinemachineCamera firstPersonCinCam;
    
    [SerializeField] private CinemachineCamera thirdPersonOrbitCinCam;
    
    [SerializeField] private CinemachineCamera thirdPersonFixedCam;
    
    [field: SerializeField] public Camera RealCam { get; private set; }
    
    
    [Header("Player References")]
    
    [SerializeField] private Transform hookPrediction;
    
    [SerializeField] private List<Transform> armBaseRigTargetRots;
    
    [field: SerializeField] public Transform CamOrientation { get; private set; }
    
    
    [Header("Input References")]
    
    public string lookActionName = "Look";
    
    public string moveActionName = "Move";

    [Header("General Cam Settings")]

    [SerializeField] private ECamType currentCamType;
    
    [Header("First Person Cam Settings")]
    
    [SerializeField] private float keyAndMouseFirstPersonLookSpeedMult = 0.5f;
    
    [SerializeField] private float gamepadFirstPersonLookSpeedMult = 1f;
    
    [Range(0,1)]
    [SerializeField] private float gamepadFirstPersonVerticalModifier = 0.5f;
    
    [Header("Third Person Orbit Cam Settings")]
    
    [SerializeField] private float playerRotSpeed = 7;
    
    [Header("Third person Fixed Cam Settings")]
    
    [SerializeField] private float keyAndMouseThirdPersonFixedLookSpeedMult = 0.5f;
    
    [SerializeField] private float gamepadThirdPersonFixedLookSpeedMult = 1f;
    
    [Range(0,1)]
    [SerializeField] private float gamepadThirdPersonFixedVerticalModifier = 0.5f;
    
    [Space]
    
    [Range(0.01f,0.25f)] 
    [SerializeField] private float sideChangeSpeed = 0.1f;

    [Header("Cam Effects Settings")]
    
    [SerializeField] private float baseFov = 100f;
    
    [Space]
    
    [SerializeField] private float baseTilt = 0f;
    
    //Dynamic, Non Serialized Below
    
    //Player References
    private Transform _orientation;

    private Grappling _grappling;
    
    private Transform _playerObj;
    
    private Rigidbody _rb;
    
    private InputAction _lookAction;
    
    private InputAction _moveAction;
    
    //Camera References
    private CinemachineThirdPersonFollow _thirdPersonFixedCamFollow;
    
    //Input
    private Vector2 _lookInput;
    
    private Vector2 _moveInput;

    private EControlScheme _controlScheme;
    
    //Rot Values

    private float _firstPersonXRot;
    private float _firstPersonYRot;
    
    private float _thirdFixedXRot;
    private float _thirdFixedYRot;
    
    private List<Vector3> _startRotDirs;
    
    private List<float> _startYRots;
    
    //Gamepad control schemes
    
    const string GAMEPADSCHEMENAME = "Gamepad";
    
    const string KANDMSCHEMENAME = "Keyboard&Mouse";
    
    // Camera Effects
    
    private float _targetFov;
    private float _targetFovIncrement;
    
    private float _targetTilt;
    private float _targetTiltIncrement;

    private float _targetSide;

    private void Awake()
    {
        // get references
        _rb = GetComponent<Rigidbody>();
        
        _thirdPersonFixedCamFollow = thirdPersonFixedCam.GetComponent<CinemachineThirdPersonFollow>();
        
        _grappling = GetComponent<Grappling>();
        
        PlayerMovement pm = GetComponent<PlayerMovement>();
        _orientation = pm.Orientation;
        _playerObj = pm.PlayerObj;
        
        
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        
        //set the control scheme
        string currentControlScheme = playerInput.currentControlScheme;
        
        if (currentControlScheme == GAMEPADSCHEMENAME)
        {
            _controlScheme = EControlScheme.Gamepad;
        }
        else if (currentControlScheme == KANDMSCHEMENAME)
        {
            _controlScheme = EControlScheme.KeyboardAndMouse;
        }
        else
        {
            Debug.LogError("Unknown control scheme, disabling");
            
            enabled = false;
        }
        
        playerInput.onControlsChanged += OnControlSchemeChanged;
        
        _moveAction = playerInput.actions.FindAction(moveActionName);
        _lookAction = playerInput.actions.FindAction(lookActionName);
        
        SwitchCamType(currentCamType);
    }
    
    private void OnControlSchemeChanged(PlayerInput playerInput)
    {
        _controlScheme = playerInput.currentControlScheme == GAMEPADSCHEMENAME ? 
            EControlScheme.Gamepad : EControlScheme.KeyboardAndMouse;
    }
    
     private void OnEnable()
    {
        _lookAction.Enable();
        
        _moveAction.Enable();
        
        //setting first person and third fixed rots based on current rots
        _firstPersonYRot = _orientation.eulerAngles.y;
        _firstPersonXRot = _orientation.eulerAngles.x;
        
        _thirdFixedYRot = _orientation.eulerAngles.y;
        _thirdFixedXRot = _orientation.eulerAngles.x;
        
        //setting target effects to base
        _targetFov = baseFov;
        _targetTilt = baseTilt;
        
        _targetSide = _thirdPersonFixedCamFollow.CameraSide;
        
        //setting arm rots
        _startRotDirs = new List<Vector3>();
        
        for (int i = 0; i < armBaseRigTargetRots.Count; i++)
        {
            _startRotDirs.Add(armBaseRigTargetRots[i].eulerAngles);
        }
    }
    
    private void OnDisable()
    {
        _lookAction.Disable();
        
        _moveAction.Disable();
    }


    private void Update()
    {

        GetInput();
        
        UpdateCamera();
        
        UpdateArms();
    }
    
    private void GetInput()
    {
        _lookInput = _lookAction.ReadValue<Vector2>();
        
        _moveInput = _moveAction.ReadValue<Vector2>();
    }
    
    internal void SwitchCamType(ECamType toCamType)
    {
        firstPersonCinCam.gameObject.SetActive( false);
        thirdPersonOrbitCinCam.gameObject.SetActive( false);
        thirdPersonFixedCam.gameObject.SetActive( false);

        switch (toCamType)
        {
            case ECamType.FirstPerson:
                
                currentCamType = ECamType.FirstPerson;
                
                firstPersonCinCam.transform.position = CamOrientation.position;
                
                firstPersonCinCam.Lens.FieldOfView = baseFov;
                
                firstPersonCinCam.Lens.Dutch = baseTilt;
                
                firstPersonCinCam.gameObject.SetActive( true);
                
                break;
            
            case ECamType.ThirdOrbit:
                
                currentCamType = ECamType.ThirdOrbit;
                
                thirdPersonOrbitCinCam.Lens.FieldOfView = baseFov;
                
                thirdPersonOrbitCinCam.Lens.Dutch = baseTilt;
                
                thirdPersonOrbitCinCam.gameObject.SetActive( true);
                
                break;
            
            case ECamType.ThirdFixed:
                
                currentCamType = ECamType.ThirdFixed;
                
                thirdPersonFixedCam.Lens.FieldOfView = baseFov;
                
                thirdPersonFixedCam.Lens.Dutch = baseTilt;
                
                thirdPersonFixedCam.gameObject.SetActive( true);
                
                break;
        }
    }

    private void UpdateCamera()
    {
        //Managing location and rotation of the camera and orientation(s)
        switch (currentCamType)
        {
            case ECamType.FirstPerson:
                
                //get look input and modify based on control scheme
                float firstLookSpeedMult = _controlScheme == EControlScheme.Gamepad ?
                gamepadFirstPersonLookSpeedMult : keyAndMouseFirstPersonLookSpeedMult;
                
                _firstPersonYRot += _lookInput.x * firstLookSpeedMult;
                
                _firstPersonXRot = _controlScheme == EControlScheme.Gamepad ?
                    _firstPersonXRot - _lookInput.y * firstLookSpeedMult * gamepadFirstPersonVerticalModifier :
                    _firstPersonXRot - _lookInput.y * firstLookSpeedMult;
                
                // make sure that you can't look up or down more than 90* degrees
                _firstPersonXRot = Mathf.Clamp(_firstPersonXRot, -89f, 89f);
                
                //rotate PlayerParent object and Orientation along only the y axis
                _orientation.rotation = Quaternion.Euler(0, _firstPersonYRot, 0);
                _playerObj.rotation = Quaternion.Euler(0, _firstPersonYRot, 0);
                
                
                //rotate RealCamTrans Orientation fully
                CamOrientation.rotation = Quaternion.Euler(_firstPersonXRot, _firstPersonYRot, 0);
                
                //apply position and rotation to the CinCamera
                firstPersonCinCam.transform.rotation = CamOrientation.rotation;
                firstPersonCinCam.transform.position = CamOrientation.position; // CamOrientation is childed to PlayerParent
                
                break;
            
            case ECamType.ThirdOrbit:
                
                //rotate RealCamTrans Orientation fully to direction from RealCamTrans to PlayerParent
                CamOrientation.rotation = Quaternion.LookRotation(_playerObj.position - new Vector3(thirdPersonOrbitCinCam.transform.position.x,
                    RealCam.transform.position.y, thirdPersonOrbitCinCam.transform.position.z));
                
                //rotate PlayerParent Orientation along only the y axis of RealCamTrans Orientation
                _orientation.rotation = Quaternion.Euler(0, CamOrientation.rotation.eulerAngles.y, 0);

                //use relative input direction to rotate PlayerParent object relative to PlayerParent Orientation
                Vector3 relativeInputDir = _orientation.forward * _moveInput.y + _orientation.right * _moveInput.x;
                
                if (relativeInputDir != Vector3.zero)
                {
                    _playerObj.forward = Vector3.Slerp(_playerObj.forward, relativeInputDir, Time.deltaTime *
                        playerRotSpeed);
                }
                
                break;
            
            case ECamType.ThirdFixed:
                
                //similar to first person, just not managing the camera position or rotation
                float thirdLookSpeedMult = _controlScheme == EControlScheme.Gamepad ?
                    gamepadThirdPersonFixedLookSpeedMult : keyAndMouseThirdPersonFixedLookSpeedMult;
                
                _thirdFixedYRot += _lookInput.x * thirdLookSpeedMult;
                
                _thirdFixedXRot = _controlScheme == EControlScheme.Gamepad ?
                    _thirdFixedXRot - _lookInput.y * thirdLookSpeedMult * gamepadThirdPersonFixedVerticalModifier :
                    _thirdFixedXRot - _lookInput.y * thirdLookSpeedMult;
                
                _thirdFixedXRot = Mathf.Clamp(_thirdFixedXRot, -89, 89);
                
                CamOrientation.transform.rotation = Quaternion.Euler(_thirdFixedXRot, _thirdFixedYRot, 0);
                
                _orientation.rotation = Quaternion.Euler(0, _thirdFixedYRot, 0);
                _playerObj.rotation = Quaternion.Euler(0, _thirdFixedYRot, 0);
                
                break;
                
        }
        
        //Managing Effects
        
        if (NeedFOVUpdate)
        {
            // Debug.Log("Current Fov: " + CurrentFov + " Target Fov: " + _targetFov);
            
            SetFOV(CurrentFov + _targetFovIncrement * Time.deltaTime);
        }
        
        if (NeedTiltUpdate)
        {
            // Debug.Log("Current Tilt: " + CurrentTilt + " Target Tilt: " + _targetTilt);
            
            SetTilt(CurrentTilt + _targetTiltIncrement * Time.deltaTime);
        }
        
        if (NeedSideUpdate)
        {
            // Debug.Log("Current Side: " + _thirdPersonFixedCamFollow.CameraSide + " Target Side: " + _targetSide);
            
            _thirdPersonFixedCamFollow.CameraSide = Mathf.Lerp(_thirdPersonFixedCamFollow.CameraSide, _targetSide, sideChangeSpeed);
        }
    }
    
    #region Fov, Tilt and CamShake

    /// function called to change target fov of camera with end result and time to take
    public void SetTargetFov(float endValue = -1, float transitionTime = 0)
    {
        //if end value is -1, set to base
        if (endValue <= -1) endValue = baseFov;
        
        _targetFov = endValue;
        
        //if zero or less, just set to target
        if (transitionTime <= 0)
        {
            SetFOV(_targetFov);
        }
        //otherwise, calculate increment for use in CamUpdate
        else
        {
            _targetFovIncrement = (_targetFov - CurrentFov) / transitionTime;
        }
    }
    
    private void SetFOV(float fov)
    {
        
        //Whether increasing or decreasing, clamp to avoid overshooting
        fov = _targetFov >= CurrentFov ?
            Mathf.Clamp(fov, CurrentFov, _targetFov) :
            Mathf.Clamp(fov, _targetFov, CurrentFov);
        
        //Can simply get RealCamTrans and set fov to fov
        switch (currentCamType)
        {
            case ECamType.FirstPerson:
                firstPersonCinCam.Lens.FieldOfView = fov;
                break;
            case ECamType.ThirdOrbit:
                thirdPersonOrbitCinCam.Lens.FieldOfView = fov;
                break;
            case ECamType.ThirdFixed:
                thirdPersonFixedCam.Lens.FieldOfView = fov;
                break;
        }
    }
    
    
    public void SetTargetTilt(float endValue = -360, float transitionTime = 0)
    { 
        if (endValue <= -360) endValue = baseTilt;
        
        _targetTilt = endValue;
        
        if (transitionTime <= 0)
        {
            SetTilt(_targetTilt);
        }
        else
        {
            _targetTiltIncrement = (_targetTilt - CurrentTilt) / transitionTime;
        }
        
        if (currentCamType == ECamType.ThirdFixed)
        {
            if (_targetTilt < baseTilt)
            {
                _targetSide = 1;
            }
            else if (_targetTilt > baseTilt)
            {
                _targetSide = 0;
            }
        }
    }
    
    private void SetTilt(float tilt)
    {
        tilt = _targetTilt >= CurrentTilt ?
            Mathf.Clamp(tilt, CurrentTilt, _targetTilt) :
            Mathf.Clamp(tilt, _targetTilt, CurrentTilt);
        
        
        switch (currentCamType)
        {
            case ECamType.FirstPerson:
                
                firstPersonCinCam.Lens.Dutch = tilt;
                
                break;
            
            case ECamType.ThirdOrbit:
                
                thirdPersonOrbitCinCam.Lens.Dutch = tilt;
                
                break;
            
            case ECamType.ThirdFixed:
                
                thirdPersonFixedCam.Lens.Dutch = tilt;
                
                break;
        }
    }

    #endregion
    
    private void UpdateArms()
    {
        hookPrediction.rotation = Quaternion.Euler(CamOrientation.eulerAngles.x, CamOrientation.eulerAngles.y, 0);
        
        //updating target arm rotations
        for ( int i = 0; i < armBaseRigTargetRots.Count; i++)
        {
            Vector3 targetRotDir = armBaseRigTargetRots[i].eulerAngles;
            
            if (_grappling.HooksActive[i])
            {
                Vector3 toGrappleDir = _grappling.HookPoints[i] - hookPrediction.position;
                
                targetRotDir.x = Quaternion.LookRotation(toGrappleDir).eulerAngles.x;
            }
            else
            {
                targetRotDir.x = CamOrientation.eulerAngles.x;
            }

            armBaseRigTargetRots[i].eulerAngles = targetRotDir;
        }
    }

    #region Properties
    
    private float CurrentFov
    {
        get
        {
            switch (currentCamType)
            {
                case ECamType.FirstPerson:
                    return firstPersonCinCam.Lens.FieldOfView;
                case ECamType.ThirdOrbit:
                    return thirdPersonOrbitCinCam.Lens.FieldOfView;
                case ECamType.ThirdFixed:
                    return thirdPersonFixedCam.Lens.FieldOfView;
                default:
                    return -1;
            }
        }
    }
    
    private float CurrentTilt
    {
        get
        {
            switch (currentCamType)
            {
                case ECamType.FirstPerson:
                    return firstPersonCinCam.Lens.Dutch;
                case ECamType.ThirdOrbit:
                    return thirdPersonOrbitCinCam.Lens.Dutch;
                case ECamType.ThirdFixed:
                    return thirdPersonFixedCam.Lens.Dutch;
                default:
                    return -360;
            }
        }
    }
    
    private bool NeedFOVUpdate
    {
        get
        {
            float diff = Mathf.Abs(CurrentFov - _targetFov);
            
            return diff > 0.01f;
        }
    }
    
    private bool NeedTiltUpdate
    {
        get
        {
            float diff = Mathf.Abs(CurrentTilt - _targetTilt);
            
            return diff > 0.01f;
        }
    }
    
    private bool NeedSideUpdate
    {
        get
        {
            if (currentCamType != ECamType.ThirdFixed) return false;
            
            float diff = Mathf.Abs(_thirdPersonFixedCamFollow.CameraSide - _targetSide);
            
            return diff > 0.01f;
        }
    }

    #endregion
}