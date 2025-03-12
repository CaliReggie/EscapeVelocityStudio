using UnityEngine;

[CreateAssetMenu(fileName = "SceneEventSO", menuName = "ScriptableObjects/SceneEventSO")]
public class SceneEventSO : ScriptableObject
{
    [Tooltip("Game states in which the event can be called.")]
    [field: SerializeField] public EGameState[] AvailableStates {get; private set;}
}
