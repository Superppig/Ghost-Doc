using Services.ObjectPools;
using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    public IMyObject myObject;
    [SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private float distance_regenerate;

    private TrackManager manager;

    private void Awake()
    {
        myObject = GetComponent<MyObject>();
    }

    private void OnEnable()
    {
        manager = GetComponentInParent<TrackManager>();
    }

    private void FixedUpdate()
    {
        transform.position += Time.fixedDeltaTime * velocity;
        if((transform.position - manager.transform.position).z > distance_regenerate)
        {
            manager.ReGenerate();
            myObject.Recycle();
        }
    }
}
