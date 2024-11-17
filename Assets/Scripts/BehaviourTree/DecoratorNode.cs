using UnityEngine;

public abstract class DecoratorNode: MNode
{
    [HideInInspector] public MNode child;
    
    public override MNode Clone()
    {
        DecoratorNode node = Instantiate(this);
        node.child = child.Clone();
        return node;
    }
}