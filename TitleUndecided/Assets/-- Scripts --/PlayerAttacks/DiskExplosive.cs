using UnityEngine;

public class DiskExplosive : Disk
{
    public GameObject explosionPrefab;

    protected override void DiskEnd()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
