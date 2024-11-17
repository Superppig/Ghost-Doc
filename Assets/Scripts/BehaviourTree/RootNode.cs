public class RootNode : MNode
{
    public MNode child;
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        return child.Update();
    }
    
    public override MNode Clone()
    {
        RootNode node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}