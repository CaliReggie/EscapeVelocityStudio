using System;
using UnityEngine;

public enum EGameState
{
    MainMenu,
    Game,
    Pause,
    GameOver
}

[CreateAssetMenu(fileName = "GameStateSO", menuName = "ScriptableObjects/GameStateSO" , order = 1)]
public class GameStateSO : ScriptableObject
{
    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.MainMenu;
    
    public void SetGameState(EGameState state, GameStateManager authorizer)
    {
        if (authorizer == GameStateManager.Instance) { GameState = state; }
    }
}
