using System;
using UnityEngine;
using UnityEngine.Events;

public class SceneEventManager : MonoBehaviour
{
    public static SceneEventManager Instance { get; private set; }
    
    [SerializeField] private SceneEvent[] sceneEvents;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            
            return;
        }
    }
    
    public bool EventExists(string eventName)
    {
        
        
        return Array.Exists(sceneEvents, sceneEvent => sceneEvent.Name == eventName);
    }
    
    public void CallEvent(SceneEvent sceneEvent)
    {
        GameStateSO gameStateSO = GameStateManager.Instance.GameStateSO;
        
        if (gameStateSO == null)
        {
            Debug.LogError("GameStateSO is not assigned in the inspector of GameStateManager!");
            
            return;
        }
        
        EGameState[] availableStates = sceneEvent.AvailableStates;
        
        if (availableStates.Length == 0)
        {
            Debug.LogError("No available states for SceneEvent!" + sceneEvent.Name);
            
            return;
        }
        
        if (!Array.Exists(availableStates, state => state == gameStateSO.GameState))
        {
            Debug.LogError("SceneEvent " + sceneEvent.Name +" cannot be called in the current game state!");
            
            return;
        }
        
        sceneEvent.OnCalled.Invoke();
    }
}

[Serializable]
public class SceneEvent
{
    [field: SerializeField] public String Name { get; private set; }
    
    [Tooltip("Game states in which the event can be called.")]
    [field: SerializeField] public EGameState[] AvailableStates {get; private set;}
    
    [field: SerializeField] public UnityEvent OnCalled {get; private set;}
}
