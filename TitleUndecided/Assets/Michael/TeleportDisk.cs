using UnityEngine;

public class TeleportDisk : Disk
{
    [Header("Teleport Disk")]
    public GameObject teleporterPrefab;
    public float playerHeight;
    private bool frozen = false;

    protected override void Movement()
    {
        if (!frozen)
        {
            base.Movement();
        }
    }
    protected override void DiskEnd()
    {
        frozen = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<Collider>().enabled = false;
        ricochet = false;
    }

    protected override void TimeCheck()
    {
        if (lifeSpan + startTime < Time.time)
        {
            Destroy(gameObject);
        }
    }

    protected void OnDestroy()
    {
        if (PlayerEquipabbles.S.teleportTarget == this.gameObject)
        {
            PlayerEquipabbles.S.teleportTarget = null;
            PlayerEquipabbles.S.activeTeleport = false;
        }
    }
}
