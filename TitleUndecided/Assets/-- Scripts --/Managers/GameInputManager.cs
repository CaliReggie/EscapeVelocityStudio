using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInputManager))]
public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance { get; private set; }
    
    [Header("Player Spawning")]
    
    [SerializeField] private GameObject uiScenePrefab;
        
    [SerializeField] private GameObject playableScenePrefab;

    [Header("Dynamic")]

    [Tooltip("Here so unity serializes the header above. Don't press the red button!")]
    [SerializeField] private bool placeholder;
    
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    
    //Private, or Non-Serialized Below
    
    // References
    
    private PlayerInputManager _playerInputManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Initialize()
    {
        // Manually Configuring Attached PlayerInputManager
        
        if (_playerInputManager == null)
        {
            _playerInputManager = GetComponent<PlayerInputManager>();
            
            if (_playerInputManager == null)
            {
                Debug.LogError("No PlayerInputManager found on GameInputManager, disabling script.");
                
                enabled = false;
                
                return;
            }
        }

        JoiningEnabled = false;
        
        _playerInputManager.playerPrefab = null;
    }
    
    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
        
        _playerInputManager.onPlayerLeft += OnPlayerLeft;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        
        _playerInputManager.onPlayerJoined -= OnPlayerJoined;
        
        _playerInputManager.onPlayerLeft -= OnPlayerLeft;
    }
    
    private void EnablePlayableSpawn()
    {
        if (playableScenePrefab == null)
        {
            Debug.LogError("No Playable Scene Prefab assigned in the inspector!");
            
            return;
        }
        
        _playerInputManager.playerPrefab = playableScenePrefab;
        
        JoiningEnabled = true;
    }
    
    private void EnableUISpawn()
    {
        if (uiScenePrefab == null)
        {
            Debug.LogError("No UI Scene Prefab assigned in the inspector!");
            
            return;
        }
        
        _playerInputManager.playerPrefab = uiScenePrefab;
        
        JoiningEnabled = true;
    }
    
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerInput = playerInput;
    }
    
    private void OnPlayerLeft(PlayerInput playerInput)
    {
        PlayerInput = null;
    }
    
    private void OnGameStateChanged(EGameState state)
    {
        switch (state)
        {
            case EGameState.MainMenu:
                
                EnableUISpawn();
                
                break;
            case EGameState.Game:
                
                EnablePlayableSpawn();
                
                break;
            case EGameState.Pause:
                // Do something
                break;
            case EGameState.GameOver:
                // Do something
                break;
            case EGameState.Reset:
                
                JoiningEnabled = false;
                
                break;
            default:
                Debug.LogError("Unhandled game state: " + state);
                break;
        }
    }
    
    public bool JoiningEnabled 
    {
        get => _playerInputManager.joiningEnabled;
        
        private set
        {
            if (_playerInputManager == null ) return;
            
            if (value == true)
            {
                
                _playerInputManager.EnableJoining();
            }
            else
            {
                _playerInputManager.DisableJoining();
            }
        }
    }
}
