using UnityEngine;

public class AirBlower : MonoBehaviour
{
    [SerializeField]
    private float impulse;

    private Vector3 Orientation => transform.TransformVector(Vector3.up);

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<BlowableObject>(out var obj))
        {
            obj.Blow(impulse * Orientation);
        }
    }


#if UNITY_EDITOR
    [SerializeField]
    private float mockTime;
    [SerializeField]
    private Transform mockTarget;
    [SerializeField]
    private float mass;
    private Vector3 locked = Vector3.zero;

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || locked == Vector3.zero)
            locked = mockTarget == null ? transform.position : mockTarget.position;

        float deltaTime = 0.02f;
        Gizmos.color = Color.red;
        Vector3 deltaV = deltaTime * Physics.gravity;
        Vector3 v = impulse / mass * Orientation;
        Vector3 p = locked;
        for (float t = 0; t < mockTime; t += deltaTime)
        {
            v += deltaV;
            p += v * deltaTime;
            Gizmos.DrawSphere(p, 0.1f);
        }
    }
#endif
}
