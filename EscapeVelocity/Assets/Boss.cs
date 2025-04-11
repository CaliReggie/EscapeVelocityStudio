using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;


public class Boss : MonoBehaviour
{
    public static Boss Instance { get; private set; } //Atypical singleton. Doesn't persist, cretead on demand
    
    [Header("References")]

    [SerializeField] private NavMeshSurface walkableSurface;
    
    [SerializeField] private Transform followTarget;

    [Header("Settings")]

    [Tooltip("Exists for serialization purposes")]
    [SerializeField] private bool emptyBool;

    [field: SerializeField] public float StoppingDistance { get; private set; } = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(Instance.gameObject); }
        
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }

    private void Update()
    {
        if (followTarget == null) { return; }
        
        Vector3 targetPosition = GetNavmeshPosition(walkableSurface, followTarget);
        
        Debug.DrawRay(targetPosition, Vector3.up * 10f, Color.red);
    }
    
    private Vector3 GetNavmeshPosition(NavMeshSurface surface, Transform target)
    {
        if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, Mathf.Infinity, surface.layerMask))
        {
            return hit.position;
        }
        else
        {
            Debug.LogWarning("NavMesh position not found for target: " + target.name);
            return transform.position; // Fallback to the transform's position
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            followTarget = newTarget;
        }
        else
        {
            Debug.LogWarning("New target is null. Cannot set target.");
        }
    }
    
    public Vector3 GetTargetPosition()
    {
        return GetNavmeshPosition(walkableSurface, followTarget);
    }
    
    public bool HasTarget => followTarget != null;
}
