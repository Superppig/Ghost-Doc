using UnityEngine;

public abstract class MNode : ScriptableObject
{
    public enum State
    {
        Running,
        Failure,
        Success
    }

    [HideInInspector] public State state = State.Running;

    [HideInInspector] public bool started = false;
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;

    public State Update()
    {
        if (!started)
        {
            started = true;
            OnStart();
        }

        state = OnUpdate();
        if (state == State.Failure || state == State.Success)
        {
            OnStop();
            started = false;
        }

        return state;
    }
    
    public virtual MNode Clone()
    {
        return Instantiate(this);
    }

    protected abstract void OnStart();
    protected abstract void OnStop();
    protected abstract State OnUpdate();
}