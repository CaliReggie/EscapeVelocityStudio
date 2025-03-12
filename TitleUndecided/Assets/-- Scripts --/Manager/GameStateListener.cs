using System;
using UnityEngine;
using UnityEngine.Events;

public class GameStateListener : MonoBehaviour
{
    [SerializeField] private StateResponse[] stateResponses;
    
    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(EGameState state)
    {
        foreach (var stateResponse in stateResponses)
        {
            if (stateResponse.State == state)
            {
                stateResponse.Response.Invoke();
                
                return;
            }
        }
    }
}

[Serializable]
public class StateResponse
{ 
    [field: SerializeField] public EGameState State {get; private set;}
    
    [field: SerializeField] public UnityEvent Response {get; private set;}
}
