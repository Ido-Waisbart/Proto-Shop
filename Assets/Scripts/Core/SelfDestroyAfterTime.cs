using UnityEngine;

public class SelfDestroyAfterTime : MonoBehaviour
{
    private float _startTime;
    [SerializeField] protected float _destroyTimeDelta;

    void Start()
    {
        _startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - _startTime > _destroyTimeDelta) {
            Destroy(gameObject);
        }
    }
}
