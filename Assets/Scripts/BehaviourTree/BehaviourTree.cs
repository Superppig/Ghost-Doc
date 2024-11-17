using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu()]
public class BehaviourTree : ScriptableObject
{
    public MNode rootNode;
    public MNode.State treeState = MNode.State.Running;
    
    public List<MNode> nodes = new List<MNode>();

    public MNode.State Update()
    {
        if (rootNode.state == MNode.State.Running)
        {
            treeState = rootNode.Update();
        }

        return treeState;
    }
    
    public MNode CreateNode(Type type)
    {
        MNode node = ScriptableObject.CreateInstance(type) as MNode;
        node.name = type.Name;
        node.guid = GUID.Generate().ToString();
        nodes.Add(node);
        
        AssetDatabase.AddObjectToAsset(node,this);
        AssetDatabase.SaveAssets();
        return node;
    }

    public void DeleteNode(MNode node)
    {
        nodes.Remove(node);
        AssetDatabase.RemoveObjectFromAsset(node);
        AssetDatabase.SaveAssets();
    }
    
    public void AddChild(MNode parent, MNode child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator)
        {
            decorator.child = child;
        }
        
        RootNode root = parent as RootNode;
        if (root)
        {
            root.child = child;
        }
        
        
        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            composite.chidren.Add(child);
        }
    }
    public void RemoveChild(MNode parent, MNode child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator)
        {
            decorator.child = null;
        }
        
        RootNode root = parent as RootNode;
        if (root)
        {
            root.child = null;
        }
        
        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            composite.chidren.Remove(child);
        }
    }
    
    public List<MNode> GetChildren(MNode parent)
    {
        List<MNode> children = new List<MNode>();
        DecoratorNode decorator = parent as DecoratorNode;
        if (decorator && decorator.child != null)
        {
            children.Add(decorator.child);
        }
        
        RootNode root = parent as RootNode;
        if (root && root.child != null)
        {
            children.Add(root.child);
        }
        
        CompositeNode composite = parent as CompositeNode;
        if (composite)
        {
            return composite.chidren;
        }

        return children;
    }
    
    public BehaviourTree Clone()
    {
        BehaviourTree tree = Instantiate(this);
        tree.rootNode = rootNode.Clone();
        return tree;
    }
}