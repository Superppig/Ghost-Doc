using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子弹属性")] 
    public float time;
    public Vector3 start;
    public Vector3 end=Vector3.zero;
    public bool ready;

    private float timer;
    private Rigidbody rb;
    void Start()
    {
        start = transform.position;
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (ready)
        {
            Fly();
        }
    }

    void Fly()
    {
        timer += Time.deltaTime;
        rb.velocity = (end - start).normalized * (Vector3.Distance(start, end) / time);
        if (timer>time)
        {
            Destroy(gameObject);
        }
    }
}
