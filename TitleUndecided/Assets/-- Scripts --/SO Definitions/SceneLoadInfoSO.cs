using UnityEngine;

[CreateAssetMenu(fileName = "SceneLoadInfoSO", menuName = "ScriptableObjects/SceneLoadInfoSO")]
public class SceneLoadInfoSO : ScriptableObject
{
    [Tooltip("The name of the scene that this info loads. Ensure the scene is included in the build settings.")]
    [field: SerializeField] public string SceneName { get; private set; }
    
    [Tooltip("The game state that the scene should load with.")]
    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.MainMenu;
}
