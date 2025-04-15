using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool useFunctionality;
    
    public EStage setStage;
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useFunctionality) return;
        
        if (other.GetComponent<PlayerMovement>() != null)
        {
            GameStateManager.Instance.SetStage(setStage);
        }
    }
}
