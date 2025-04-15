using System.Collections;
using UnityEngine;
using Unity.Collections;

public class bullet : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject target;
    public float speed = 9.0f;
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        StartCoroutine(bulletDestroy());


    }

    public IEnumerator bulletDestroy()
    {
        yield return new WaitForSeconds(7.0f);
        Destroy(this.gameObject);
    }
}
