using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
    
    public void CallEvent(SceneEventSO otherSceneEventSO)
    {
        if (otherSceneEventSO == null)
        {
            Debug.LogError("SceneEventSO passed is null!");
            
            return;
        }

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager doesn't exist!");
            
            return;
        }

        if (GameStateManager.Instance.GameStateSO == null)
        {
            Debug.LogError("GameStateSO is not assigned in the GameStateManager!");
            
            return;
        }

        //checking our SceneEvents to see if it has the SceneEvent with corresponding SceneEventSO
        foreach (var sceneEvent in sceneEvents)
        {
            //if our scene exists and is the same as the SceneEventSO we are looking for
            if (sceneEvent.SceneEventSO != null && sceneEvent.SceneEventSO == otherSceneEventSO)
            {
                //if the current game state is part of acceptable states for the SceneEventSO
                if (sceneEvent.SceneEventSO.AvailableStates.Contains(GameStateManager.Instance.GameStateSO.GameState))
                {
                    sceneEvent.OnCalled.Invoke();
                    
                    return;
                }
            }
        }
    }
}

[Serializable]
public class SceneEvent
{
    [field: SerializeField] public SceneEventSO SceneEventSO {get; private set;}
    [field: SerializeField] public UnityEvent OnCalled {get; private set;}
}
