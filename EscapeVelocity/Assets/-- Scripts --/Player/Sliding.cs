using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


[RequireComponent(typeof(PlayerMovement))]
public class Sliding : MonoBehaviour
{
    [Header("Input References")]
    
    [SerializeField] private string slideActionName = "Crouch";

    [Header("Timings")]
    
    [SerializeField] private float slideCooldown = 0.5f;
    
    [Header("Force Settings")]
    
    [SerializeField] private bool dynamicInitialForce = true;
    
    [SerializeField] private float initialForce = 10f;
    
    [SerializeField] private float additiveForce = 200f;

    [Header("Behaviour Settings")]
    
    [SerializeField] private float slideColliderHeight = 1f;

    [SerializeField] private float slideColliderCenterY = -0.5f;
    
    [SerializeField] private bool reverseCoyoteTime = true; //pressed in air triggers when Grounded
    
    //Dynamic, Non-Serialized Below
    
    //References
    private Rigidbody _rb;
    private PlayerMovement _pm; // script reference to the PlayerMovement script
    
    private InputAction _slideAction; // the input action for Sliding
    
    //Player Collider
    private CapsuleCollider _playerColl;
    
    private float _startCollHeight;
    
    private float _startCollCenterY;
    
    private Vector3 _startInputDirection;
    
    //State
    private bool _bufferSlide;
    private bool _readyToSlide = true;

    private void Awake()
    {
        // get references
        _rb             = GetComponent<Rigidbody>();
        _pm             = GetComponent<PlayerMovement>();
        _playerColl = GetComponent<CapsuleCollider>();
        
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        
        _slideAction = playerInput.actions.FindAction(slideActionName);
    }

    private void Start()
    {
        //store start collider settings
        _startCollHeight = _playerColl.height;
        
        _startCollCenterY = _playerColl.center.y;

        _readyToSlide = true;
    }

    private void OnEnable()
    {
        _slideAction.Enable();
    }
    
    private void OnDisable()
    {
        _slideAction.Disable();
    }

    private void Update()
    {
        // if you press down the slide key while moving and not crouching,
        // try to start Sliding, can be denied by movement extension
        if (_slideAction.triggered && (_pm.RawMoveInput != Vector2.zero) && !_pm.Crouching)
        {
            if (reverseCoyoteTime) _bufferSlide = true;
            
            else if (_pm.Grounded && _readyToSlide) _bufferSlide = true;
        }
        
        // slide buffering
        if (_bufferSlide && _pm.Grounded && _readyToSlide)
        {
            StartSlide();
            
            _bufferSlide = false;
        }

        // if you release the slide key while Sliding -> StopSlide
        if (!_slideAction.IsPressed() && _pm.Sliding)
        {
            if (reverseCoyoteTime) _bufferSlide = false;

            StopSlide();
            
            return;
        }
        
        // un sliding if no longer Grounded
        if (_pm.Sliding && !_pm.Grounded)
        {
            StopSlide();
        }
            
    }

    private void FixedUpdate()
    {
        // make sure that Sliding movement is continuously called while Sliding
        if (_pm.Sliding) SlidingMovement();
    }
    
    private void StartSlide()
    {
        if (!_pm.SpeedAllowsState(PlayerMovement.MovementMode.sliding))
            return;

        if (!_pm.Grounded) return;

        // this causes the PlayerParent to change to MovementMode.Sliding
        _pm.Sliding = true;
        _readyToSlide = false;

        // change PlayerParent collider size
        _playerColl.height = slideColliderHeight;
        
        _playerColl.center = new Vector3(_playerColl.center.x, slideColliderCenterY, _playerColl.center.z);
        
        // if dynamic, initial force is current vel plus initial force, otherwise just initial force
        Vector3 startForce = dynamicInitialForce ? 
            _rb.linearVelocity + _rb.linearVelocity.normalized * initialForce : 
            _rb.linearVelocity.normalized * initialForce;
        
        // after shrinking, you'll be a bit in the air, so add downward force to hit the ground again
        // you don't really notice this while playing
        startForce += Vector3.down * 5f;
        
        // add the force
        _rb.AddForce(startForce, ForceMode.Impulse);
    }

    private void SlidingMovement()
    {
        Vector3 moveInput = _pm.OrientedMoveInput;

        // Mode 1 - Sliding Normal
        // slide time is limited
        if(!_pm.IsOnSlope() || _rb.linearVelocity.y > -0.1f)
        {
            // add force in the direction of your input
            _rb.AddForce(moveInput * additiveForce, ForceMode.Force);
        }

        // Mode 2 - Sliding down slopes
        // can slide for as long as the slope lasts
        else
        {
            // add force in the direction of your keyboard input
            _rb.AddForce(_pm.SlopeMoveDirection(moveInput) * additiveForce, ForceMode.Force);
        }
    }

    private void StopSlide()
    {
        _pm.Sliding = false;

        // reset PlayerParent collider size
        _playerColl.height = _startCollHeight;
        
        _playerColl.center = new Vector3(_playerColl.center.x, _startCollCenterY, _playerColl.center.z);

        StartCoroutine(ResetSlideDelayed(slideCooldown));
    }

    private void ResetSlide()
    {
        _readyToSlide = true;
    }
    
    private IEnumerator ResetSlideDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        _readyToSlide = true;
    }
}
