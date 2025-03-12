using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    public event Action<EGameState> OnGameStateChanged;
    
    [SerializeField] public SceneEventManager CurrentSceneEventManager {get; private set;}
    
    [field: SerializeField] public GameStateSO GameStateSO { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            
            return;
        }
        
        if (GameStateSO == null)
        {
            Debug.LogError("GameStateSO is not assigned in the inspector!");
            
            return;
        }
    }
    
    private void OnGameStateChangedInternal(EGameState state)
    {
        switch (state)
        {
            case EGameState.MainMenu:
                // Do something
                break;
            case EGameState.Game:
                // Do something
                break;
            case EGameState.Pause:
                // Do something
                break;
            case EGameState.GameOver:
                // Do something
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
    
    private bool CanEnterGameState(EGameState state)
    {
        return true;
    }
    
    public void EnterGameState(EGameState state)
    {
        if (!CanEnterGameState(state)) { return; }
        
        GameStateSO.SetGameState(state, this);
        
        OnGameStateChanged?.Invoke(state);
    }
}