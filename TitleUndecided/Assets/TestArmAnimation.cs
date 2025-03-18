using UnityEngine;
using UnityEngine.InputSystem;

public class TestArmAnimation : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    
    [SerializeField]
    private string _animAction;
    
    private int _animActionID = Animator.StringToHash("ArmTrigger");
    
    private Animator _animator;
    
    private InputAction _animActionReference;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        _animActionReference = playerInput.actions[_animAction];
    }
    
    private void OnEnable()
    {
        _animActionReference.Enable();
    }
    
    private void OnDisable()
    {
        _animActionReference.Disable();
    }
    
    private void Update()
    {
        //if action triggered, set trigger
        if (_animActionReference.triggered)
        {
            _animator.SetTrigger(_animActionID);
        }
    }
}
