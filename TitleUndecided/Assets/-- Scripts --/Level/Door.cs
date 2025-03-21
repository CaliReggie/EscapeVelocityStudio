using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    [Header("Inscribed")]
    public bool isEnter;
    public int lastLevelNumber;
    public int nextLevelNumber;
    public MeshRenderer doorMesh;
    public BoxCollider doorColl;

    [Header("Dynamic")]
    public Vector3 spawnPoint;

    void OnTriggerEnter(Collider coll) {
        BoxCollider box = GetComponent<BoxCollider>();
        if (coll.gameObject.layer == LayerMask.NameToLayer("whatIsPlayer")) {
            if (!isEnter) {
                doorMesh.enabled = true;
                doorColl.enabled = true;
                box.enabled = false;
                spawnPoint = coll.transform.position;
            } else {
                doorMesh.enabled = false;
                doorColl.enabled = false;
                box.enabled = false;
            }
        }
    }
}
