using System;
using System.Collections.Generic;
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

    [SerializeField]
    private Transform spawn1;
    
    [SerializeField]
    private Transform spawn2;
    
    [SerializeField]
    private Transform spawn3;
    

    [Header("Settings")]

    [Tooltip("Exists for serialization purposes")]
    [SerializeField] private bool emptyBool;
    
    [SerializeField] private float maxHealth;

    [field: SerializeField] public float StoppingDistance { get; private set; } = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(Instance.gameObject); }
        
        Instance = this;
        
        transform.parent.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }

    private void Update()
    {
        if (followTarget == null) { return; }
        
        // Vector3 targetPosition = GetNavmeshPosition(walkableSurface, followTarget);
        
        Vector3 targetPosition = followTarget.position;
        
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
        followTarget = newTarget;
        
        List<Turret> turrets = new List<Turret>(GetComponentsInChildren<Turret>());
        
        if (turrets.Count > 0)
        {
            foreach (Turret turret in turrets)
            {
                turret.SetTarget(newTarget);
            }
        }
    }
    
    public Vector3 GetTargetPosition()
    {
        return GetNavmeshPosition(walkableSurface, followTarget);
    }
    
    public void MoveToSpawn(EStage stage, bool active)
    {
        transform.parent.gameObject.SetActive(false);
        
        Enemy enemy = GetComponent<Enemy>();

        float setHealth = maxHealth;
        
        switch (stage)
        {
            case EStage.One:
                transform.position = spawn1.position;
                transform.rotation = spawn1.rotation;
                setHealth = maxHealth;
                break;
            case EStage.Two:
                transform.position = spawn2.position;
                transform.rotation = spawn2.rotation;
                setHealth = maxHealth * (2/3);
                break;
            case EStage.Three:
                transform.position = spawn3.position;
                transform.rotation = spawn3.rotation;
                setHealth = maxHealth * (1/3);
                break;
            default:
                Debug.LogWarning("Invalid stage provided.");
                break;
        }
        
        enemy.health = setHealth;
        
        transform.parent.gameObject.SetActive(active);
    }
    
    public bool HasTarget => followTarget != null;
}
