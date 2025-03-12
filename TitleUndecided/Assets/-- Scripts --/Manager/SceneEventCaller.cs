using System;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneEventCaller : MonoBehaviour
{
    [Tooltip("SceneEvent name from SceneEventManager in scene to be called.")]
    [SerializeField] private String sceneEventName;
    
    [Header("Debug")]
    
    [Tooltip("During runtime, click this to try calling the SceneEvent.")]
    [SerializeField] private bool callEvent;
    
    // Private, Or Non - Serialized Below
    private SceneEvent _sceneEvent;

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
        if (CanCall())
        {
            SceneEventManager.Instance.CallEvent(_sceneEvent);
        }
    
        return;
        
        bool CanCall()
        {
            return (_sceneEvent != null && SceneEventManager.Instance != null);
        }
    }
}
