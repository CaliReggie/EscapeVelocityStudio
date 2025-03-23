using UnityEngine;

public class MeleeAttack : Weapon
{
    public float duration = 0.5f;
    public float totalRotation = 180f;
    private float _rotPerSec;
    private GameObject _attackObj;
    private float _startTime;
    void Start()
    {
        _rotPerSec = totalRotation / duration;
        _attackObj = transform.GetChild(0).gameObject;
        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, _rotPerSec * Time.deltaTime,0);
        if (Time.time - _startTime > duration)
        {
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        CheckForEnemy(collision);
    }
}
