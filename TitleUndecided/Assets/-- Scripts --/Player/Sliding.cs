using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


// Dave MovementLab - Sliding
///
// Content:
/// - Sliding ability
/// 
// Note:
/// Sliding is basically Crouching while moving
/// Crouching is handled by the PlayerMovement script


public class Sliding : MonoBehaviour
{
    [Header("Player References")]
    
    public Transform orientation; // Orientation object inside the PlayerParent
    private Rigidbody rb;
    private PlayerMovement pm; // script reference to the PlayerMovement script
    
    [Header("Input Reference")]
    
    public string slideActionName = "Crouch";
    
    public InputAction slideAction; // the input action for Sliding

    [Header("Timings")]
    
    public float slideCooldown = 0.5f;
    
    public float minSlideTime = 0.2f;
    public float maxSlideTime = 0.75f; // how long the slide maximally lasts
    
    [Header("Force Settings")]
    
    public bool useDynamicSlideForce = true; //if true, PlayerParent momentum on slide equal to that of when pressed
    
    public float nonDynamicSlideForce = 200f;
    
    [FormerlySerializedAs("crouchColliderHeight")]
    [Header("Behaviour Settings")]
    
    public float slideColliderHeight = 1f;
    
    [FormerlySerializedAs("playerColliderCenterY")]
    [FormerlySerializedAs("crouchColliderCenterY")]
    public float slideColliderCenterY = -0.5f;
    
    public bool reverseCoyoteTime = true; //held in air triggers when Grounded
     
    private Vector3 startInputDirection;
    //Dynamic, Non-Serialized Below
    
    //Player Collider
    private CapsuleCollider playerCollider;
    
    private float startCollHeight;
    
    private float startCollCenterY;
    
    //Timing
    private float slideTimer;
    
    
    //Inputs
    private float horizontalInput;
    private float verticalInput;
    
    //State
    private bool bufferSlide;
    private bool readyToSlide = true;
    private bool stopSlideAsap;
    
    // Dynamic Slide Force
    private float dynamicStartForce;

    private void Start()
    {
        // get references
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<CapsuleCollider>();
        
        //store start collider settings
        startCollHeight = playerCollider.height;
        
        startCollCenterY = playerCollider.center.y;

        readyToSlide = true;
        
        PlayerInput playerInput = GetComponent<PlayerInput>();
        
        slideAction = playerInput.actions.FindAction(slideActionName);
        
    }

    private void OnEnable()
    {
        slideAction.Enable();
    }
    
    private void OnDisable()
    {
        slideAction.Disable();
    }

    private void Update()
    {
        // get the W,A,S,D keyboard inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // if you press down the slide key while moving -> StartSlide
        if (slideAction.triggered && (horizontalInput != 0 || verticalInput != 0))
        {
            if (reverseCoyoteTime) bufferSlide = true;
            else if (pm.Grounded && readyToSlide) bufferSlide = true;
        }

        // if you release the slide key while Sliding -> StopSlide
        if (!slideAction.IsPressed() && pm.Sliding)
        {
            if (reverseCoyoteTime) bufferSlide = false;

            if (pm.Sliding) stopSlideAsap = true;
        }

        // slide buffering
        if (bufferSlide && pm.Grounded && readyToSlide)
        {
            StartSlide();
            bufferSlide = false;
        }

        // unslide if slide key was released and minSlideTime exceeded
        if (stopSlideAsap && maxSlideTime - slideTimer > minSlideTime)
        {
            stopSlideAsap = false;
            StopSlide();
        }

        // unsliding if no longer Grounded
        if (pm.Sliding && !pm.Grounded)
            StopSlide();
    }

    private void FixedUpdate()
    {
        // make sure that Sliding movement is continuously called while Sliding
        if (pm.Sliding) SlidingMovement();
    }
    
    public void StartSlide()
    {
        if (!pm.IsStateAllowed(PlayerMovement.MovementMode.sliding))
            return;

        if (!pm.Grounded) return;

        // this causes the PlayerParent to change to MovementMode.Sliding
        pm.Sliding = true;
        readyToSlide = false;

        // change PlayerParent collider size
        playerCollider.height = slideColliderHeight;
        
        playerCollider.center = new Vector3(playerCollider.center.x, slideColliderCenterY, playerCollider.center.z);
        
        // store the start dynamic force
        dynamicStartForce = rb.linearVelocity.magnitude;
        
        // after shrinking, you'll be a bit in the air, so add downward force to hit the ground again
        /// you don't really notice this while playing
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        // set the slideTimer
        slideTimer = maxSlideTime;

        // idk, feels weird but the idea would be ok I guess
        // startInputDirection = Orientation.forward * pm.verticalInput + Orientation.right * pm.horizontalInput;
    }

    private void SlidingMovement()
    {
        // calculate the direction of your keyboard input relative to the players Orientation (where the PlayerParent is looking)
        Vector3 inputDirection = Vector3.Normalize(orientation.forward * verticalInput + orientation.right * horizontalInput);
        
        //altering slide force before sending in if desired
        if (useDynamicSlideForce)
        {
            nonDynamicSlideForce = dynamicStartForce;
        }

        // Mode 1 - Sliding Normal
        /// slide time is limited
        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            // add force in the direction of your keyboard input
            rb.AddForce(inputDirection * nonDynamicSlideForce, ForceMode.Force);

            // count down timer
            slideTimer -= Time.deltaTime;
        }

        // Mode 2 - Sliding down slopes
        /// can slide for as long as the slope lasts
        else
        {
            // add force in the direction of your keyboard input
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * nonDynamicSlideForce, ForceMode.Force);
        }

        // stop Sliding again if the timer runs out
        if (slideTimer <= 0) StopSlide();
    }

    public void StopSlide()
    {
        pm.Sliding = false;

        // reset PlayerParent collider size
        playerCollider.height = startCollHeight;
        
        playerCollider.center = new Vector3(playerCollider.center.x, startCollCenterY, playerCollider.center.z);

        Invoke(nameof(ResetSlide), slideCooldown);
    }

    private void ResetSlide()
    {
        readyToSlide = true;
    }
}
