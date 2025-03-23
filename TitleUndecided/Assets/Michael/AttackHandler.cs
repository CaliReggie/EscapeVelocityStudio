using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(WeaponDefinitions))]
public class AttackHandler : MonoBehaviour
{
    [Header("Inscribed")]
    public WeaponDefinitions definitions;
    public string attackInputName = "CustomAttack";
    public string teleportInputName = "CustomTeleport";
    public string swapInputName = "CustomSwap";
    public Transform spawnPoint;
    public GameObject realCam;
    public bool activeTeleport = false;
    public GameObject teleportTarget;
    private PlayerMovement _playerMovement;
    private PlayerInput _playerInput;
    private InputAction _attackAction;
    private InputAction _teleportAction;
    private InputAction _swapAction;

    private float attackDuration;
    private float lastAttackTime;
    public static AttackHandler S;
    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        if (S != null)
        {
            Debug.LogError("Attack Handler already Set");
        }
        else
        {
            S = this;
        }
        _playerInput = GetComponentInParent<PlayerInput>();
        _attackAction = _playerInput.actions.FindAction(attackInputName);
        _teleportAction = _playerInput.actions.FindAction(teleportInputName);
        _swapAction = _playerInput.actions.FindAction(swapInputName);
    }

    void Update()
    {
        
        if (Time.time - lastAttackTime > attackDuration && PlayerStateManager.GetAttackingEquipable() != Equippables.None)
        {
            PlayerStateManager.SetAttackingEquipable(Equippables.None);
        }

        if (PlayerStateManager.GetAttackingEquipable() != Equippables.None)
        {
            return;
        }
        if (_swapAction.triggered)
        {
            PlayerStateManager.GoToNextEquippable();
        }
        if (_attackAction.triggered)
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
        switch (PlayerStateManager.GetEquippable())
        {
            case Equippables.Disk: 
                Disk disk = definitions.GetWeapon<Disk>();
                attackingWeapon = disk;
                GameObject diskIns = Instantiate(disk.gameObject, spawnPoint.position, Quaternion.identity);
                disk.transform.forward = realCam.transform.forward;
                Rigidbody rb = diskIns.GetComponent<Rigidbody>();
                rb.AddForce(realCam.transform.forward * disk.speed, ForceMode.Impulse);
                break;
            case Equippables.StickyDisk:
                break;
            case Equippables.TeleportDisk:
                TeleportDisk teleportDisk = definitions.GetWeapon<TeleportDisk>();
                attackingWeapon = teleportDisk;
                teleportTarget = Instantiate(teleportDisk.gameObject, spawnPoint.position, Quaternion.identity);
                teleportDisk.transform.forward = realCam.transform.forward;
                Rigidbody teleportRb = teleportTarget.GetComponent<Rigidbody>();
                teleportRb.AddForce(realCam.transform.forward * teleportDisk.speed, ForceMode.Impulse);
                activeTeleport = true;
                break;
            case Equippables.Melee:
                MeleeAttack meleeAttack = definitions.GetWeapon<MeleeAttack>();
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
            PlayerStateManager.SetAttackingEquipable(attackingWeapon.equippableType);
            attackDuration = attackingWeapon.lockedInAttackDur;
            lastAttackTime = Time.time;
        }
    }
    public void Teleport()
    {
        transform.position = teleportTarget.transform.position;
        GetComponent<Rigidbody>().linearVelocity = teleportTarget.GetComponent<Rigidbody>().linearVelocity;
        Destroy(teleportTarget);
        S.activeTeleport = false;
    }

    public static void ActivateTeleport(GameObject teleportTar)
    {
        S.teleportTarget = teleportTar;
        S.activeTeleport = true;
    }
}
