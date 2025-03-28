using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public enum Equippables
{
    Grapple,
    Disk,
    TeleportDisk,
    StickyDisk,
    Melee,
    None,
}

public class PlayerEquipabbles : MonoBehaviour
{
    public static PlayerEquipabbles S;
    
    [Header("Input References")]
    
    public string attackInputName = "CustomAttack";
    
    public string teleportInputName = "CustomTeleport";

    [Header("Player References")]
    
    public Transform spawnPoint;
    
    [Header("Camera References")]
    
    public GameObject realCam;
    
    [Header("Equippable Definitions")]
    
    [SerializeField] private Equippable[] equippableDefinitions;
    
    //Dynamic, or Non-Serialized Below
    
    //Timing
    private float _timeUnlocked;
    
    //Teleport
    [Header("Dynamic")]
    public bool activeTeleport = false;
    
    public GameObject teleportTarget;
    
    //Player References
    private PlayerMovement _playerMovement;
    
    private PlayerInput _playerInput;
    
    private InputAction _attackAction;
    
    private InputAction _teleportAction;
    
    void Awake()
    {
        if (S != null)
        {
            Debug.LogError("Attack Handler already Set");
        }
        else
        {
            S = this;
        }
        
        _playerMovement = GetComponent<PlayerMovement>();
        
        _playerInput = GetComponentInParent<PlayerInput>();
        
        _attackAction = _playerInput.actions.FindAction(attackInputName);
        
        _teleportAction = _playerInput.actions.FindAction(teleportInputName);
        
        EquipByType(Equippables.Grapple);
    }

    void Update()
    {
        if (Time.time < _timeUnlocked) return; //Not allowing actions while locked
        
        if (_attackAction.triggered && (Time.time > CurrentEquippable.TimeRefreshed)) //Allow if triggered and refreshed
        {
            Attack();
        }

        if (_teleportAction.triggered)
        {
            if (activeTeleport)
            {
                Teleport();
            }
        }
    }
    
    public void Attack()
    {
        Weapon attackingWeapon = null;
        
        switch (CurrentEquippable.EquippableType)
        {
            case Equippables.Disk: 
                Disk disk = Equippable.FindWeaponFromEquippables<Disk>(equippableDefinitions);
                attackingWeapon = disk;
                GameObject diskIns = Instantiate(disk.gameObject, spawnPoint.position, Quaternion.identity);
                disk.transform.forward = realCam.transform.forward;
                Rigidbody rb = diskIns.GetComponent<Rigidbody>();
                rb.AddForce(realCam.transform.forward * disk.speed, ForceMode.Impulse);
                break;
            case Equippables.StickyDisk:
                break;
            case Equippables.TeleportDisk:
                TeleportDisk teleportDisk = Equippable.FindWeaponFromEquippables<TeleportDisk>(equippableDefinitions);
                attackingWeapon = teleportDisk;
                teleportTarget = Instantiate(teleportDisk.gameObject, spawnPoint.position, Quaternion.identity);
                teleportDisk.transform.forward = realCam.transform.forward;
                Rigidbody teleportRb = teleportTarget.GetComponent<Rigidbody>();
                teleportRb.AddForce(realCam.transform.forward * teleportDisk.speed, ForceMode.Impulse);
                activeTeleport = true;
                break;
            case Equippables.Melee:
                MeleeAttack meleeAttack = Equippable.FindWeaponFromEquippables<MeleeAttack>(equippableDefinitions);
                attackingWeapon = meleeAttack;
                GameObject meleeIns = Instantiate(meleeAttack.gameObject, _playerMovement.PlayerObj.transform);
                float lookVertAngle = realCam.transform.localRotation.eulerAngles.x;
                if (lookVertAngle > 180)
                {
                    lookVertAngle -= 360;
                }
                lookVertAngle = Mathf.Clamp(lookVertAngle, -30f, 30f);
                meleeIns.transform.localRotation = Quaternion.Euler(0f, 90f - meleeAttack.totalRotation / 2.0f, -lookVertAngle);
                break;
        }
        if (attackingWeapon is not null)
        {
            _timeUnlocked = Time.time + CurrentEquippable.LockOnUseDuration;
            
            CurrentEquippable.OnUsed();
        }
    }
    public void Teleport()
    {
        transform.position = teleportTarget.transform.position;
        GetComponent<Rigidbody>().linearVelocity = teleportTarget.GetComponent<Rigidbody>().linearVelocity;
        Destroy(teleportTarget);
        S.activeTeleport = false;
    }
    
    public void EquipByType(Equippables type)
    {
        Equippable equippable = Array.Find(equippableDefinitions, e => e.EquippableType == type);
        
        if (equippable is null) return;
        
        CurrentEquippable = equippable;
    }
    
    public Equippable CurrentEquippable {get; private set; }
    
    public bool IsUnlocked => Time.time >= _timeUnlocked;
}

[Serializable]
public class Equippable
{
    [Tooltip("The type of equippable this is, works with equip types in UI Wheel")]
    [field: SerializeField] public Equippables EquippableType { get; private set; }
    
    [Tooltip("Do not use if equip type is Grapple")]
    [field: SerializeField] public Weapon AssociatedWeapon { get; private set; }
    
    [Tooltip("How long to wait before being able to use again. Do not use if equip type is Grapple")]
    [field: SerializeField] public float WeaponRefreshDuration { get; private set; }
    
    //Dynamic, or Non-Serialized Below
    public float LockOnUseDuration => AssociatedWeapon == null ? 0 : AssociatedWeapon.lockedInAttackDur;
    
    public float TimeRefreshed { get; private set; }
    
    //Instance Methods
    public void OnUsed()
    {
        if (AssociatedWeapon == null) return;

        TimeRefreshed = Time.time + WeaponRefreshDuration;
    }
    
    //For if you want to make a weapon refresh besides process of waiting
    public void Refresh()
    {
        TimeRefreshed = Time.time;
    }
    
    //General Class Method
    public static T FindWeaponFromEquippables<T>(Equippable[] equippableArray) where T : Weapon
    {
        foreach (Equippable equippable in equippableArray)
        {
            if (equippable.AssociatedWeapon is T specificWeapon)
            {
                return specificWeapon;
            }
        }
        
        return null;
    }
}
