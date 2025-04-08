using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
public enum EEquippableClass
{
    Grapple,
    CombatDisk,
    UtilityDisk,
    Melee
}

public class PlayerEquipabbles : MonoBehaviour
{
    public static PlayerEquipabbles S;
    
    [Header("Input References")]
    
    public string attackInputName = "CustomAttack";
    public string attackSecondaryInputName = "CustomSecondaryAttack";
    public string teleportInputName = "CustomTeleport";

    [Header("Player References")]
    
    public Transform spawnPoint;
    
    [Header("Camera References")]
    
    public GameObject realCam;
    
    [FormerlySerializedAs("equippableDefinitions")]
    [Header("Equippable Definition Pool")]
    
    [Tooltip("Definitions for types and stats of equippables. If a definition is grapple, " +
             "ignore the weaponType and refresh duration fields")]
    [SerializeField] private Equippable[] equippablePrimaryDefinitions;
    [SerializeField] private Equippable[] equippableSecondaryDefinitions;
    
    //This is where we could set the preferences for what gets equipped depending on the class the UI selects
    [Header("Equippable Pool Preferences")]
    
    public EEquippableWeapon combatClassPreference = EEquippableWeapon.DamageDisk;
    
    public EEquippableWeapon utilityClassPreference = EEquippableWeapon.TeleportDisk;
    
    public EEquippableWeapon meleeClassPreference = EEquippableWeapon.Melee;


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
    private InputAction _attackSecondaryAction;
    private InputAction _teleportAction;

    private static event Action UseEquipmentEvent;
    private static event Action UseEquipmentSecondaryEvent;
    
    //Ammunition Script
    private Ammuninition ammunition;
    void Awake()
    {
        if (S != null)
        {
            Debug.LogError("UseCurrentEquippable Handler already Set");
        }
        else
        {
            S = this;
        }
        ammunition = GetComponent<Ammuninition>();
        _playerMovement = GetComponent<PlayerMovement>();
        
        _playerInput = GetComponentInParent<PlayerInput>();
        
        _attackAction = _playerInput.actions.FindAction(attackInputName);

        _attackSecondaryAction = _playerInput.actions.FindAction(attackSecondaryInputName);
        
        _teleportAction = _playerInput.actions.FindAction(teleportInputName);
        
        EquipByClass(EEquippableClass.Grapple);
    }

    void Update()
    {
        if (Time.time < _timeUnlocked) return; //Not allowing actions while locked
        
        if (_attackAction.triggered && (Time.time > CurrentPrimaryEquippable.TimeRefreshed)) //Allow if triggered and refreshed and has ammo needed
        {
            UseCurrentEquippable();
        }

        else if (_attackSecondaryAction.triggered && (Time.time > CurrentSecondaryEquippable.TimeRefreshed))
        {
            UseSecondaryEquippable();
        }

        if (_teleportAction.triggered)
        {
            if (activeTeleport)
            {
                Teleport();
            }
        }
    }

    public void UseSecondaryEquippable()
    {
        if (UseEquipmentSecondaryEvent == null)
        {
            return;
        }
        if (ammunition.UseAmmo(_currentSecondaryEquippable.AmmoCost))
        {
            UseEquipmentSecondaryEvent?.Invoke();
            _timeUnlocked = Time.time + CurrentPrimaryEquippable.LockOnUseDuration;
            CurrentSecondaryEquippable.OnUsed();
        }
    }
    
    public void UseCurrentEquippable()
    {
        if (UseEquipmentEvent == null)
        {
            return;
        }
        if (ammunition.UseAmmo(_currentPrimaryEquippable.AmmoCost))
        {
            UseEquipmentEvent?.Invoke();
            _timeUnlocked = Time.time + CurrentPrimaryEquippable.LockOnUseDuration;
            CurrentPrimaryEquippable.OnUsed();
        }
    }

    private void SpawnPrimaryEquipment()
    {
        Instantiate(CurrentPrimaryEquippable.AssociatedWeapon.gameObject, realCam.transform);
    }
    private void SpawnSecondaryEquipment()
    {
        Instantiate(CurrentSecondaryEquippable.AssociatedWeapon.gameObject, realCam.transform);
    }
    
    public void Teleport()
    {
        transform.position = teleportTarget.transform.position;
        GetComponent<Rigidbody>().linearVelocity = teleportTarget.GetComponent<Rigidbody>().linearVelocity;
        Destroy(teleportTarget);
        S.activeTeleport = false;
    }
    
    public void EquipByClass(EEquippableClass targetClass)
    {
        //iterating and looking for equippable with same class and matching weapon if not grapple
        Equippable equippable = null;
        
        foreach (Equippable e in equippablePrimaryDefinitions)
        {
            if (e.EquippableClass == targetClass)
            {
                switch (targetClass)
                {
                    case EEquippableClass.Grapple:
                        
                        equippable = e;
                        
                        break;
                    
                    case EEquippableClass.CombatDisk:
                        
                        if (e.AssociatedWeapon.weaponType == combatClassPreference)
                        {
                            equippable = e;
                        }
                        
                        break;
                    
                    case EEquippableClass.UtilityDisk:
                        
                        if (e.AssociatedWeapon.weaponType == utilityClassPreference)
                        {
                            equippable = e;
                        }
                        
                        break;
                    case EEquippableClass.Melee:
                        
                        if (e.AssociatedWeapon.weaponType == meleeClassPreference)
                        {
                            equippable = e;
                        }
                        
                        break;
                }
            }
        }
        
        if (equippable is null) return;
        
        CurrentPrimaryEquippable = equippable;
    }

    public Equippable FindEquippableByClass(EEquippableClass targetClass, Equippable[] equippables)
    {
        foreach (Equippable e in equippables)
        {
            if (e.EquippableClass == targetClass)
            {
                return e;
            }
        }

        return null;
    }
    
    private Equippable _currentPrimaryEquippable;

    public Equippable CurrentPrimaryEquippable
    {
        get => _currentPrimaryEquippable;
        private set
        {
            _currentPrimaryEquippable = value;
            CurrentSecondaryEquippable = FindEquippableByClass(CurrentPrimaryEquippable.EquippableClass, equippableSecondaryDefinitions);
            if (CurrentPrimaryEquippable.EquippableClass == EEquippableClass.Grapple || CurrentPrimaryEquippable == null)
            {
                UseEquipmentEvent = null;
            }
            else
            {
                UseEquipmentEvent = SpawnPrimaryEquipment;
            }
        }
    }

    private Equippable _currentSecondaryEquippable;
    public Equippable CurrentSecondaryEquippable
    {
        get =>_currentSecondaryEquippable;
        private set
        {
            _currentSecondaryEquippable = value;
            if (CurrentSecondaryEquippable.EquippableClass == EEquippableClass.Grapple || CurrentPrimaryEquippable == null)
            {
                UseEquipmentSecondaryEvent = null;
            }
            else
            {
                UseEquipmentSecondaryEvent = SpawnSecondaryEquipment;
            }
        }
    }
    public bool IsUnlocked => Time.time >= _timeUnlocked;
}

[Serializable]
public class Equippable
{
    [Tooltip("The type of equippable class this is, works with equip types in UI Wheel")]
    [field: SerializeField] public EEquippableClass EquippableClass { get; private set; }
    
    [Tooltip("Do not use if equip type is Grapple")]
    [field: SerializeField] public Weapon AssociatedWeapon { get; private set; }
    
    [Tooltip("How long to wait before being able to use again. Do not use if equip type is Grapple")]
    [field: SerializeField] public float WeaponRefreshDuration { get; private set; }
    [field: SerializeField] public float AmmoCost { get; private set; }
    
    //Dynamic, or Non-Serialized Below
    public float LockOnUseDuration => AssociatedWeapon == null ? 0 : AssociatedWeapon.lockedInAttackDur;
    
    public float TimeRefreshed { get; private set; }
    
    //Instance Methods
    public void OnUsed()
    {
        if (AssociatedWeapon == null) return;

        TimeRefreshed = Time.time + WeaponRefreshDuration;
    }
    
    //For if you want to make a weaponType refresh besides process of waiting
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
