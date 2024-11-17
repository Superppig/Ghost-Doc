public class SequencerNode : CompositeNode
{
    private int current;
    protected override void OnStart()
    {
        current = 0;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        var child = chidren[current];
        switch (child.Update())
        {
            case State.Running:
                return State.Running;
                break;
            case State.Success:
                current++;
                break;
            case State.Failure:
                return State.Failure;
                break;
        }
        return current == chidren.Count ? State.Success : State.Running;
    }
}