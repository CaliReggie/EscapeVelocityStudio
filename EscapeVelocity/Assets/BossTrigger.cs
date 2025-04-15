using System;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public EStage stage;

    public bool makeActive;
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
        
        if (player != null)
        {
            Boss.Instance.gameObject.SetActive(true);
            Boss.Instance.MoveToSpawn(stage, makeActive);
            
            if (makeActive)
            {
                Boss.Instance.SetTarget(player.transform);
            }else
            {
                Boss.Instance.SetTarget(null);
            }
        }
    }
}
