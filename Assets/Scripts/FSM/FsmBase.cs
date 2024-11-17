using System;
using UnityEngine;

public abstract class FsmBase
{
    public int FsmID
    {
        get;
        private set;
    }

    public Type currentState;

    public FsmBase(int fsmID)
    {
        FsmID = fsmID;
    }

    public abstract void Start(Type stateType);
    public abstract void Shutdown();
}