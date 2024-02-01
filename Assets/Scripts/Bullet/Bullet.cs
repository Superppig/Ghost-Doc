
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 dir;
    public float speed;
    public float damage;
    public float lifeTime;
    
    
    protected float timer;
    protected Rigidbody rb;
    
    protected virtual void  Start()
    {
        rb = GetComponent<Rigidbody>();
        timer = 0f;
    }
    protected virtual void Update()
    {
        Move();
    }
    protected virtual void Move()
    {
        rb.velocity = dir * speed;
        timer += Time.deltaTime;
        if (timer>lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
