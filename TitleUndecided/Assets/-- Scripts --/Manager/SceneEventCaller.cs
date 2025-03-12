using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneEventCaller : MonoBehaviour
{
    [Tooltip("SceneEventSO that corresponds to desired SceneEvent set in SceneEventManager.")]
    [SerializeField] private SceneEventSO sceneEventSO;
    
    [Header("Debug")]
    
    [Tooltip("During runtime, click this to try calling the SceneEvent.")]
    [SerializeField] private bool callEvent;

    private void OnValidate()
    {
        if (callEvent && Application.isPlaying)
        {
            CallEvent();
            
            callEvent = false;
        }
    }

    public void CallEvent()
    {
        if (sceneEventSO == null)
        {
            Debug.LogError("SceneEventSO is not assigned in the inspector!");
            
            return;
        }
        
        SceneEventManager.Instance.CallEvent(sceneEventSO);
    }
}
