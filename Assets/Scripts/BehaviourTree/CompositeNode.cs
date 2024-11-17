using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode: MNode
{
    [HideInInspector] public List<MNode> chidren = new List<MNode>();
    public override MNode Clone()
    {
        CompositeNode node = Instantiate(this);
        node.chidren = chidren.ConvertAll(c => c.Clone());
        return node;
    }
}