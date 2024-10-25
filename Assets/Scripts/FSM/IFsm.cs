using UnityEngine;

public interface IFsm<T> where T : class
{
    T Owner
    {
        get;
    }
    
}