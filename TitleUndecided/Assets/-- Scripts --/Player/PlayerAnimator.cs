using UnityEngine;
using UnityEngine.Serialization;

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

public class PlayerAnimator : MonoBehaviour
{
    public enum EMovementStyle
    {
        Strafe,
        Forward
    }
    
    [Header("Animator References")]
    
    [SerializeField] private Animator animator;
    
    [Header("Player References")]
    
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private PlayerCam playerCamScript;
    
    [Header("Animation Behaviours")]
    
    [SerializeField] private EMovementStyle movementStyle;
    
    //Dynamic, Non-Serialized Below
    
    private EPlayerAnimationState _currentState;
    
    private PlayerMovement _pm;
    
    #region Animation Variable Hashes

    private readonly int _something = Animator.StringToHash("Something");

    #endregion
}