using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerUIInput : MonoBehaviour
{
    [Header("Input References")]
    
    [SerializeField] private string pauseAction = "Pause";
    
    [SerializeField] private string equipWheelUseAction = "EquipWheelUse";
    
    [SerializeField] private string equipWheelNavigateAction = "EquipWheelNavigate";
    
    [SerializeField] private string playerActionMap = "Player";
    
    [SerializeField] private string uiActionMap = "UI";
    
    //Private, or Non-Serialized Below
    
    private InputAction _pauseAction;
    
    private InputAction _equippableWheelUseAction;
    
    private InputAction _equippableWheelNavigateAction;
    
    private UIManager _uiManager;
    
    private GameStateManager _gameStateManager;
    
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        _pauseAction = _playerInput.actions.FindAction(pauseAction);
        
        _equippableWheelUseAction = _playerInput.actions.FindAction(equipWheelUseAction);
        
        _equippableWheelNavigateAction = _playerInput.actions.FindAction(equipWheelNavigateAction);
    }
    
    private void OnEnable()
    {
        _uiManager = UIManager.Instance;
        
        _gameStateManager = GameStateManager.Instance;
        
        _gameStateManager.OnGameStateChanged += OnGameStateChanged;
        
        _pauseAction.Enable();
        
        _equippableWheelUseAction.Enable();
        
        _equippableWheelNavigateAction.Enable();
        
        _uiManager.EquippableWheel.EquipWheelAction = _equippableWheelNavigateAction;
    }
    
    private void OnDisable()
    {
        _gameStateManager.OnGameStateChanged -= OnGameStateChanged;
        
        _pauseAction.Disable();
        
        _equippableWheelUseAction.Disable();
        
        _equippableWheelNavigateAction.Disable();
    }
    
    private void OnGameStateChanged(EGameState toState, EGameState fromState)
    {
        switch (toState)
        {
            case EGameState.Game:
                
                SwitchToMap(playerActionMap);
                
                break;
            
            case EGameState.Pause:
                
                SwitchToMap(uiActionMap);
                
                break;
        }
    }
    
    private void Update()
    {
        if (_pauseAction.triggered)
        {
            _gameStateManager.EnterGameState(EGameState.Pause, GameStateManager.Instance.GameStateSO.GameState);
        }
        
        if (_equippableWheelUseAction.triggered)
        {
            _uiManager.EquippableWheel.gameObject.SetActive(true);
        }
        
        // Turn off if wheel is active and not being used
        if (_uiManager.EquippableWheel.gameObject.activeSelf && !_equippableWheelUseAction.IsPressed())
        {
            _uiManager.EquippableWheel.gameObject.SetActive(false);
        }
    }
    
    private void SwitchToMap(string map)
    {
        _playerInput.SwitchCurrentActionMap(map);
    }
}
