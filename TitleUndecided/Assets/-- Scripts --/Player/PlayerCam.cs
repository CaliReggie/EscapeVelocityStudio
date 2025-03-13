using System;
using System.Collections;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.Serialization;

#if UNITY_EDITOR

//maaking custom GUI buttons
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

public class PlayerCam : MonoBehaviour
{
    public enum EControlScheme
    {
        KeyboardAndMouse,
        Gamepad
    }
    
    [Header("Cam References")]
    
    public Camera realCam; 
    
    public CinemachineCamera firstPersonCinCam;
    
    public CinemachineCamera thirdPersonOrbitCinCam;
    
    public CinemachineCamera thirdPersonFixedCam;
    
    [Header("Player References")]
    
    public Transform orientation;
    
    public Transform camOrientation;
    
    public Transform player;
    
    public Transform playerObj;
    
    public Transform grappleRig;
    
    [Header("Input References")]
    
    public string lookActionName = "Look";
    
    public string moveActionName = "Move";
    
    public InputAction lookAction;
    
    public InputAction moveAction;
    
    [Header("General Cam Settings")]
    
    public ECamType camType = ECamType.FirstPerson;
    
    public LayerMask firstPersonRenderMask = -1;
    
    public LayerMask thirdPersonRenderMask = -1;
    
    [Header("First Person Cam Settings")]
    
    public float keyAndMouseFirstPersonLookSpeedMult = 0.5f;
    
    public float gamepadFirstPersonLookSpeedMult = 0.5f;
    
    [Range(0,1)]
    public float gamepadFirstPersonVerticalModifier = 0.5f;
    
    [Header("Third Person Orbit Cam Settings")]
    
    public float playerRotSpeed = 7;
    
    [Header("Third person Fixed Cam Settings")]
    
    public float keyAndMouseThirdPersonFixedLookSpeedMult = 0.5f;
    
    public float gamepadThirdPersonFixedLookSpeedMult = 0.5f;
    
    [Range(0,1)]
    public float gamepadThirdPersonFixedVerticalModifier = 0.5f;

    [Header("Cam Effects Settings")]
    public float baseFov = 100f;
    
    [Space]
    
    public float baseTilt = 0f;
    
    //Dynamic, Non Serialized Below

    public event Action<ECamType> OnSwitchCamType;
    
    //References
    private CinemachineThirdPersonFollow thirdPersonFixedCamFollow;
    
    private Rigidbody rb;
    
    //Input
    private Vector2 lookInput;
    
    private Vector2 moveInput;

    private EControlScheme controlScheme;
    
    //Rot Values

    private float firstPersonXRot;
    private float firstPersonYRot;
    
    private float thirdFixedXRot;
    private float thirdFixedYRot;
    
    //Gamepad control schemes
    
    const string gamepadControlSchemeName = "Gamepad";
    
    const string keyAndMouseControlSchemeName = "Keyboard&Mouse";

    private void Awake()
    {
        if (lookAction == null)
        {
            Debug.LogError("Look action not set in ThirdPersonCameraController");
            
            Destroy(gameObject);
        }
        
        if (moveAction == null)
        {
            Debug.LogError("Move action not set in ThirdPersonCameraController");
            
            Destroy(gameObject);
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        
        Cursor.visible = false;
        
        PlayerInput playerInput = GetComponent<PlayerInput>();
        
        //set the control scheme
        string currentControlScheme = playerInput.currentControlScheme;
        
        if (currentControlScheme == gamepadControlSchemeName)
        {
            controlScheme = EControlScheme.Gamepad;
        }
        else if (currentControlScheme == keyAndMouseControlSchemeName)
        {
            controlScheme = EControlScheme.KeyboardAndMouse;
        }
        else
        {
            Debug.LogError("Unknown control scheme, destroying attached Player Input GameObject");
            
            Destroy(gameObject);
        }
        
        playerInput.onControlsChanged += OnControlSchemeChanged;
        
        moveAction = playerInput.actions.FindAction(moveActionName);
        lookAction = playerInput.actions.FindAction(lookActionName);
    }
    
    private void Start()
    {
        // get the components
        rb = GetComponent<Rigidbody>();
        
        // get the third person fixed cam follow component
        thirdPersonFixedCamFollow = thirdPersonFixedCam.GetComponent<CinemachineThirdPersonFollow>();

        SwitchCamType(camType);
    }
    
    private void OnControlSchemeChanged(PlayerInput playerInput)
    {
        controlScheme = playerInput.currentControlScheme == gamepadControlSchemeName ? 
            EControlScheme.Gamepad : EControlScheme.KeyboardAndMouse;
    }
    
     private void OnEnable()
    {
        lookAction.Enable();
        
        moveAction.Enable();
    }
    
    private void OnDisable()
    {
        lookAction.Disable();
        
        moveAction.Disable();
    }


    private void Update()
    {

        GetInput();
        
        ManageCamera();
        
        ManageGrappleGear();
    }
    
    private void GetInput()
    {
        lookInput = lookAction.ReadValue<Vector2>();
        
        moveInput = moveAction.ReadValue<Vector2>();
    }
    
    internal void SwitchCamType(ECamType toCamType)
    {
        firstPersonCinCam.gameObject.SetActive( false);
        thirdPersonOrbitCinCam.gameObject.SetActive( false);
        thirdPersonFixedCam.gameObject.SetActive( false);

        switch (toCamType)
        {
            case ECamType.FirstPerson:
                
                camType = ECamType.FirstPerson;
                
                realCam.cullingMask = firstPersonRenderMask;
                
                firstPersonCinCam.transform.position = camOrientation.position;
                
                firstPersonCinCam.gameObject.SetActive( true);
                
                break;
            
            case ECamType.ThirdOrbit:
                
                camType = ECamType.ThirdOrbit;
                
                realCam.cullingMask = thirdPersonRenderMask;
                
                thirdPersonOrbitCinCam.gameObject.SetActive( true);
                
                break;
            
            case ECamType.ThirdFixed:
                
                camType = ECamType.ThirdFixed;
                
                realCam.cullingMask = thirdPersonRenderMask;
                
                thirdPersonFixedCam.gameObject.SetActive( true);
                
                break;
        }
        
        OnSwitchCamType?.Invoke(camType);
    }

    private void ManageCamera()
    {
        
        switch (camType)
        {
            case ECamType.FirstPerson:
                
                //get look input and modify based on control scheme
                float firstLookSpeedMult = controlScheme == EControlScheme.Gamepad ?
                gamepadFirstPersonLookSpeedMult : keyAndMouseFirstPersonLookSpeedMult;
                
                firstPersonYRot += lookInput.x * firstLookSpeedMult;
                
                firstPersonXRot = controlScheme == EControlScheme.Gamepad ?
                    firstPersonXRot - lookInput.y * firstLookSpeedMult * gamepadFirstPersonVerticalModifier :
                    firstPersonXRot - lookInput.y * firstLookSpeedMult;
                
                // make sure that you can't look up or down more than 90* degrees
                firstPersonXRot = Mathf.Clamp(firstPersonXRot, -89f, 89f);
                
                //rotate player object and orientation along only the y axis
                orientation.rotation = Quaternion.Euler(0, firstPersonYRot, 0);
                playerObj.rotation = Quaternion.Euler(0, firstPersonYRot, 0);
                
                
                //rotate cam orientation fully
                camOrientation.rotation = Quaternion.Euler(firstPersonXRot, firstPersonYRot, 0);
                
                //apply position and rotation to the CinCamera
                firstPersonCinCam.transform.rotation = camOrientation.rotation;
                firstPersonCinCam.transform.position = camOrientation.position; // camOrientation is childed to player
                
                break;
            
            case ECamType.ThirdOrbit:
                
                //rotate cam orientation fully to direction from cam to player
                camOrientation.rotation = Quaternion.LookRotation(player.position - new Vector3(thirdPersonOrbitCinCam.transform.position.x,
                    realCam.transform.position.y, thirdPersonOrbitCinCam.transform.position.z));
                
                //rotate player orientation along only the y axis of cam orientation
                orientation.rotation = Quaternion.Euler(0, camOrientation.rotation.eulerAngles.y, 0);

                //use relative input direction to rotate player object relative to player orientation
                Vector3 relativeInputDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;
                
                if (relativeInputDir != Vector3.zero)
                {
                    playerObj.forward = Vector3.Slerp(playerObj.forward, relativeInputDir, Time.deltaTime *
                        playerRotSpeed);
                }
                
                break;
            
            case ECamType.ThirdFixed:
                
                //similar to first person, just not managing the camera position or rotation
                float thirdLookSpeedMult = controlScheme == EControlScheme.Gamepad ?
                    gamepadThirdPersonFixedLookSpeedMult : keyAndMouseThirdPersonFixedLookSpeedMult;
                
                thirdFixedYRot += lookInput.x * thirdLookSpeedMult;
                
                thirdFixedXRot = controlScheme == EControlScheme.Gamepad ?
                    thirdFixedXRot - lookInput.y * thirdLookSpeedMult * gamepadThirdPersonFixedVerticalModifier :
                    thirdFixedXRot - lookInput.y * thirdLookSpeedMult;
                
                thirdFixedXRot = Mathf.Clamp(thirdFixedXRot, -89, 89);
                
                camOrientation.transform.rotation = Quaternion.Euler(thirdFixedXRot, thirdFixedYRot, 0);
                
                orientation.rotation = Quaternion.Euler(0, thirdFixedYRot, 0);
                playerObj.rotation = Quaternion.Euler(0, thirdFixedYRot, 0);
                
                break;
                
        }
    }
    
    private void ManageGrappleGear()
    {
        switch(camType)
        {
            case ECamType.FirstPerson:
                
                grappleRig.rotation = Quaternion.Euler(firstPersonXRot, firstPersonYRot, 0);
                
                break;
            
            case ECamType.ThirdOrbit:
                
                Vector3 orbitViewDir = player.position - thirdPersonOrbitCinCam.transform.position;
                
                grappleRig.rotation = Quaternion.LookRotation(orbitViewDir);
                
                break;
            
            case ECamType.ThirdFixed:
                
                grappleRig.rotation = Quaternion.Euler(thirdFixedXRot, thirdFixedYRot, 0);
                
                break;
        }
    }


    /// double click the field below to show all fov, tilt and realCam shake code
    /// Note: For smooth transitions I use the free DoTween Asset!
    #region Fov, Tilt and CamShake

    /// function called when starting to wallrun or starting to dash
    /// a simple function that just takes in an endValue, and then smoothly sets the cameras fov to this end value
    public void DoFov(float endValue = -360, float transitionTime = -1)
    {
        //stop coroutine if running
        StopCoroutine(nameof(ChangeFOV));
        
        //if end value is -1, set to base
        if (endValue == -360) endValue = baseFov;
        
        //if transition time is -1, instantly set fov, otherwise use time
        if (transitionTime == -1)
        {
            SetFOV(endValue);
        }
        else
        {
            //Start coroutine to change fov
            StartCoroutine(ChangeFOV(endValue, transitionTime));
        }
    }
    
    private void SetFOV(float fov)
    {
        
        //Can simply get cam and set fov to fov
        switch (camType)
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
    
    private IEnumerator ChangeFOV(float endValue, float transitionTime)
    {
        CinemachineCamera cam = null;
        
        switch (camType)
        {
            case ECamType.FirstPerson:
                cam = firstPersonCinCam;
                break;
            case ECamType.ThirdOrbit:
                cam = thirdPersonOrbitCinCam;
                break;
            case ECamType.ThirdFixed:
                cam = thirdPersonFixedCam;
                break;
            default:
                Debug.LogError("No cam type set in ChangeFOV");
                
                yield break;
        }
        
        float incrementAmount = (endValue - cam.Lens.FieldOfView) / transitionTime;
        
        float timeStop = Time.time + transitionTime;
        
        while (Time.time < timeStop)
        {
            cam.Lens.FieldOfView += incrementAmount * Time.deltaTime;
            
            yield return null;
        }
    }
    
    
    public void DoTilt(float zTilt = -360, float transitionTime = -1)
    { 
        StopCoroutine(nameof(ChangeTilt));
        
        if (zTilt == -360) zTilt = baseTilt;
        
        if (transitionTime == -1)
        {
            SetTilt(zTilt);
        }
        else
        {
            //Start coroutine to change tilt
            StartCoroutine(ChangeTilt(zTilt, transitionTime));
        }
    }
    
    private void SetTilt(float tilt)
    {

        switch (camType)
        {
            case ECamType.FirstPerson:
                firstPersonCinCam.Lens.Dutch = tilt;
                break;
            case ECamType.ThirdOrbit:
                thirdPersonOrbitCinCam.Lens.Dutch = tilt;
                break;
            case ECamType.ThirdFixed:
                
                thirdPersonFixedCam.Lens.Dutch = tilt;
                
                //if tilting to new state, change side
                if (tilt != baseTilt)
                {
                    thirdPersonFixedCamFollow.CameraSide = tilt < 0 ? 1 : 0;
                }
                break;
        }
    }
    
    private IEnumerator ChangeTilt (float endValue, float transitionTime)
    {
        CinemachineCamera cam = null;

        float targetFixedSide = thirdPersonFixedCamFollow.CameraSide;
        
        switch (camType)
        {
            case ECamType.FirstPerson:
                cam = firstPersonCinCam;
                break;
            case ECamType.ThirdOrbit:
                cam = thirdPersonOrbitCinCam;
                break;
            case ECamType.ThirdFixed:
                
                cam = thirdPersonFixedCam;
                
                if (endValue != baseTilt)
                {
                    targetFixedSide = endValue < 0 ? 1 : 0;
                }
                
                break;
            default:
                Debug.LogError("No cam type set in ChangeTilt");
                
                yield break;
        }
        
        float tiltIncrement = (endValue - cam.Lens.Dutch) / transitionTime;
        
        float sideIncrement;
        
        //cam range is slider 0-1, so we need to convert the target side to this range
        sideIncrement = (targetFixedSide - thirdPersonFixedCamFollow.CameraSide) / transitionTime;
        
        float timeStop = Time.time + transitionTime;
        
        while (Time.time < timeStop)
        {
            cam.Lens.Dutch += tiltIncrement * Time.deltaTime;
            
            if (camType == ECamType.ThirdFixed)
            {
                thirdPersonFixedCamFollow.CameraSide += sideIncrement * Time.deltaTime;
            }
            
            yield return null;
        }
    }
    
    public void DoShake(float amplitude, float frequency)
    {
        //Change
    }
    
    public void ResetShake()
    {
        //Change
    }

    #endregion
}