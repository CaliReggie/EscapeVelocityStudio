using UnityEngine;
using UnityEngine.SceneManagement;

public class Death : MonoBehaviour
{
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.GetComponent<PlayerMovement>() != null)
        {
            GameStateManager.Instance.GameOver(false);
        }
    }
}
