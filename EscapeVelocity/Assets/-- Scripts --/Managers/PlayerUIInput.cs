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

    [SerializeField] private string lookAction = "Look";
    
    [SerializeField] private string camSwitchAction = "CamSwitch";
    
    [SerializeField] private string playerActionMap = "Player";
    
    [SerializeField] private string uiActionMap = "UI";
    
    [SerializeField] private PlayerCam playerCam;
    
    //Private, or Non-Serialized Below
    
    private InputAction _pauseAction;
    
    private InputAction _equippableWheelUseAction;
    
    private InputAction _equippableWheelNavigateAction;

    private InputAction _lookAction;
    
    private InputAction _camSwitchAction;
    
    private UIManager _uiManager;
    
    private GameStateManager _gameStateManager;
    
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        _pauseAction = _playerInput.actions.FindAction(pauseAction);

        _lookAction = _playerInput.actions.FindAction(lookAction);
        
        _equippableWheelUseAction = _playerInput.actions.FindAction(equipWheelUseAction);
        
        _equippableWheelNavigateAction = _playerInput.actions.FindAction(equipWheelNavigateAction);
        
        _camSwitchAction = _playerInput.actions.FindAction(camSwitchAction);
    }
    
    private void OnEnable()
    {
        _uiManager = UIManager.Instance;
        
        _gameStateManager = GameStateManager.Instance;
        
        _gameStateManager.OnGameStateChanged += OnGameStateChanged;
        
        _pauseAction.Enable();
        
        _equippableWheelUseAction.Enable();
        
        _equippableWheelNavigateAction.Enable();
        
        _camSwitchAction.Enable();
        
        _uiManager.UISelectWheel.EquipWheelAction = _equippableWheelNavigateAction;
    }
    
    private void OnDisable()
    {
        _gameStateManager.OnGameStateChanged -= OnGameStateChanged;
        
        _pauseAction.Disable();
        
        _equippableWheelUseAction.Disable();
        
        _equippableWheelNavigateAction.Disable();
        
        _camSwitchAction.Disable();
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
        if (_pauseAction.triggered && !_gameStateManager.Paused)
        {
            _gameStateManager.EnterGameState(EGameState.Pause, GameStateManager.Instance.GameStateSO.GameState);
        }
        
        if (_equippableWheelUseAction.triggered && PlayerEquipabbles.S.IsUnlocked)
        {
            _uiManager.UISelectWheel.gameObject.SetActive(true);

            Time.timeScale = 0.1f;

            _lookAction.Disable();
        }
        
        // Turn off if wheel is active and not being used
        if (_uiManager.UISelectWheel.gameObject.activeSelf && !_equippableWheelUseAction.IsPressed())
        {
            _uiManager.UISelectWheel.gameObject.SetActive(false);

            Time.timeScale = 1f;

            _lookAction.Enable();
        }
        
        if (_camSwitchAction.triggered)
        {
            playerCam.SwapFirstOrFixed();
        }
    }
    
    private void SwitchToMap(string map)
    {
        _playerInput.SwitchCurrentActionMap(map);
    }
}
