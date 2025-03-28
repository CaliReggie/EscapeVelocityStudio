using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

public enum EPlayerAnimationState
{
    Idle,
    Walk,
    Run,
    Jump,
    Fall,
    Crouch,
    Slide,
    Dash,
    Grapple,
    Swing
}

public enum EEquippableAnimationState
{
    Idle,
    InUse
}

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimator : MonoBehaviour
{
    
    [Header("Animator References")]
    
    [SerializeField] private Animator playerAnimator;
    
    [SerializeField] private Animator leftArmAnimator;
    
    [SerializeField] private Animator rightArmAnimator;
    
    [Header("Player References")]
    
    [SerializeField] private string leftUseArmAction;
    
    [SerializeField] private string rightUseArmAction;
    
    //Dynamic, or Non-Serialized Below
    
    //Player References
    
    private InputAction _leftUseArmActionReference;
    
    private InputAction _rightUseArmActionReference;
    
    //Anim variable hashes
    
    private int _useLeftAnimID = Animator.StringToHash("UsedLeftArm");
    
    private int _useRightAnimID = Animator.StringToHash("UsedRightArm");

    private void Awake()
    {
        //get references
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        
        _leftUseArmActionReference = playerInput.actions[leftUseArmAction];
        
        _rightUseArmActionReference = playerInput.actions[rightUseArmAction];
    }
    
    private void OnEnable()
    {
        _leftUseArmActionReference.Enable();
        
        _rightUseArmActionReference.Enable();
    }
    
    private void OnDisable()
    {
        _leftUseArmActionReference.Disable();
        
        _rightUseArmActionReference.Disable();
    }
    
    private void Update()
    {
        //if action triggered, set trigger
        if (_leftUseArmActionReference.triggered)
        {
            leftArmAnimator.SetTrigger(_useLeftAnimID);
        }
        
        if (_rightUseArmActionReference.triggered)
        {
            rightArmAnimator.SetTrigger(_useRightAnimID);
        }
    }
}