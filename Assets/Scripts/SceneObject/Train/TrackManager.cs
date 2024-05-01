using Services;
using Services.ObjectPools;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    private IObjectManager objectManager;
    private IMyObject firstSegment;
    [SerializeField]
    private Vector3 offset;

    private void Awake()
    {
        objectManager = ServiceLocator.Get<IObjectManager>();
    }

    private void Start()
    {
        for (int i = -2; i < 3; i++)
        {
            objectManager.Activate("TrackSegment", new Vector3(0, 0, 45f * i), Vector3.zero, transform);
        }
        firstSegment = GetComponentInChildren<MyObject>();
    }

    public void ReGenerate()
    {
        Debug.Log(firstSegment.Transform.position + offset);
        firstSegment = objectManager.Activate("TrackSegment", firstSegment.Transform.position + offset, Vector3.zero, transform);
    }
}
