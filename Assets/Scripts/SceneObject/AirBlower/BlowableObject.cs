using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class BlowableObject : MonoBehaviour
{
    protected Rigidbody _rigidbody;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public virtual void Blow(Vector3 impulse)
    {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.AddForce(impulse, ForceMode.Impulse);
    }
}
