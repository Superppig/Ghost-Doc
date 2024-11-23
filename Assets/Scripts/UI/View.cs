using UnityEngine;

public abstract class View : MonoBehaviour
{
    public abstract void Init();
    public virtual void Hide() => gameObject.SetActive(false);
    public virtual void Show() => gameObject.SetActive(true);
}