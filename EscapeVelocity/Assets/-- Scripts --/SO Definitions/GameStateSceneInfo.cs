using UnityEngine;
using UnityEngine.Serialization;

public enum EStage
{
    One,
    Two,
    Three
}

/// <summary>
/// Container for information about what scene to load, and relevant information about the game state to load with.
/// </summary>
[CreateAssetMenu(fileName = "GameStateSceneInfo", menuName = "ScriptableObjects/GameStateSceneInfo")]
public class GameStateSceneInfo : ScriptableObject
{
    [Tooltip("The name of the scene that this info loads. Ensure the scene is included in the build settings.")]
    [field: SerializeField] public string SceneName { get; private set; }
    
    [Tooltip("The game state that the scene should load with.")]
    [field: SerializeField] public EGameState SceneStartState { get; private set; } = EGameState.MainMenu;
    
    [SerializeField] public Vector3 stageOnePos;
    [SerializeField] public Vector3 stageOneRot;
    
    [SerializeField] public Vector3 stageTwoPos;
    [SerializeField] public Vector3 stageTwoRot;
    
    [SerializeField] public Vector3 stageThreePos;
    [SerializeField] public Vector3 stageThreeRot;

    [field: SerializeField] public float FullGameTime { get; private set; } = 180f;
    
    public EStage StartStage { get; private set; } = EStage.One;
    
    [field: SerializeField] public EStage CurrentStage { get; set; } = EStage.One;
}
