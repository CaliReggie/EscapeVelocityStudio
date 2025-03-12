using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the game state. On a global game scope, it utilizes the GameStateSO to both set and get the current game
/// state. On a per-scene scope, 
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    public event Action<EGameState> OnGameStateChanged;
    
    [field: SerializeField] public GameStateSO GameStateSO { get; private set; }
    
    [field: SerializeField] public SceneEventManager CurrentSceneEventManager {get; private set;}
    
    [field: SerializeField] public SceneLoadInfoSO CurrentSceneLoadInfoSO {get; private set;}
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            DontDestroyOnLoad(gameObject);
            
            if (CurrentSceneLoadInfoSO != null) {SetSceneFromInfoSO(CurrentSceneLoadInfoSO);}
            
            SceneManager.sceneLoaded += OnSceneLoaded;
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

    private void ThisOnGameStateChanged(EGameState state)
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

    private bool ValidInfo(SceneLoadInfoSO info)
    {
        if (info == null)
        {
            Debug.LogError("Scene info is null. " +
                           "Please assign a SceneLoadInfoSO object to the info variable.");
            return false;
        }
        
        if (info.SceneName == "")
        {
            Debug.LogError("Scene name in scene info is empty. " +
                           "Please assign a Scene Name to the SceneLoadInfoSO object passed in.");
            return false;
        }
        
        return true;
    }
    
    private void SetSceneFromInfoSO(SceneLoadInfoSO info)
    {
        if (!ValidInfo(info)) { return; }
        
        CurrentSceneLoadInfoSO = info;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (GameStateSO.GameState)
        {
            //Do something
        } 
    }
    
    public void EnterGameState(EGameState state)
    {
        if (!CanEnterGameState(state)) { return; }
        
        GameStateSO.SetGameState(state, this);
        
        ThisOnGameStateChanged(state);
        
        OnGameStateChanged?.Invoke(state);
    }
    
    public void LoadSceneFromInfoSO(SceneLoadInfoSO info)
    {
        if (!ValidInfo(info)) { return; }
        
        SetSceneFromInfoSO(info);
        
        SceneManager.LoadScene(info.SceneName);
    }
    
    public void ReloadScene()
    {

        if (CurrentSceneLoadInfoSO== null)
        {
            Debug.LogError("CurrentSceneLoadInfoSO is null!");
            
            return;
        }
        
        SceneManager.LoadScene(CurrentSceneLoadInfoSO.SceneName);
    }
    public void QUIT_GAME()
    {
        Application.Quit();
    }
}