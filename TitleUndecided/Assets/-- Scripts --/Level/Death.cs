using UnityEngine;
using UnityEngine.SceneManagement;

public class Death : MonoBehaviour
{
    void OnCollisionEnter(Collision coll) {
        if (coll.gameObject.layer == LayerMask.NameToLayer("whatIsPlayer")) {
            GameObject state = GameObject.Find("GameStateManager");
            GameObject input = GameObject.Find("GameInputManager");
            GameObject canvas = GameObject.Find("Canvas");
            Destroy(state);
            Destroy(input);
            Destroy(canvas);
            SceneManager.LoadScene("Level");
        }
    }
}
