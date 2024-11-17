using UnityEngine;
public interface IGrabObject
{
    public Transform GetTransform();
    public void Grabbed();
    public void Released();
    public void Fly(Vector3 dir, float force);
    public bool CanGrab();

    public bool CanUse();
    public void Use();
}