using UnityEngine;
using System.Collections.Generic;
public class DiskWall : Disk
{
    private static List<GameObject> walls = new List<GameObject>();
    public GameObject wall;
    protected override void OnCollisionEnter(Collision collision)
    {
        if ((recentHitObject == collision.collider.gameObject && ((Time.time - recentHitTime) < 0.25f) || collision.gameObject == gameObject))
        {
            return;
        } 
        base.OnCollisionEnter(collision);
        if (Utils.IsLayerInLayerMask(collision.gameObject.layer, ricochetMask))
        {
            GameObject wallIns =Instantiate(wall, collision.contacts[0].point, Quaternion.identity);
            wallIns.transform.up = collision.contacts[0].normal;
            wallIns.transform.position = collision.contacts[0].point + wallIns.transform.up * wallIns.transform.localScale.y/2;
            walls.Add(wallIns);
            if (walls.Count > 3)
            {
                Destroy(walls[0]);
                walls.RemoveAt(0);
            }
            Destroy(this.gameObject);
        }
    }
}
