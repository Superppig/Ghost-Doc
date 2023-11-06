using Services;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraTrans;
    void Update()
    {
        transform.position = cameraTrans.position;
    }
}
