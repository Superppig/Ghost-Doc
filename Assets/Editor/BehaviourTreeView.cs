using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class BehaviourTreeView: GraphView
{
    public Action<NodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }

    private BehaviourTree tree;
    public BehaviourTreeView()
    {
        Insert(0, new GridBackground());
        
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BehaviourTreeEditor.uss");
        styleSheets.Add(styleSheet);
    }

    NodeView FindNodeView(MNode node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }
    
    internal void PopulateView(BehaviourTree tree)
    {
        this.tree = tree;
        
        
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;
        
        if(tree.rootNode == null)
        {
            tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }
        
        

        tree.nodes.ForEach(n => CreateNodeView(n));
        tree.nodes.ForEach(n =>
        {
            var children = tree.GetChildren(n);
            children.ForEach(c =>
            {
                NodeView parentView = FindNodeView(n);
                NodeView childView = FindNodeView(c);
                
                Edge edge = parentView.output.ConnectTo(childView.input);
                AddElement(edge);
            });
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort=>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove!= null)
        {
            graphViewChange.elementsToRemove.ForEach(e =>
            {
                NodeView nodeView = e as NodeView;
                if (nodeView != null)
                {
                    tree.DeleteNode(nodeView.node);
                }
                
                Edge edge = e as Edge;
                if (edge != null)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.RemoveChild(parentView.node,childView.node);
                }
            });
        }
        
        if(graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(e =>
            {
                NodeView parentView = e.output.node as NodeView;
                NodeView childView = e.input.node as NodeView;
                tree.AddChild(parentView.node,childView.node);
            });
        }
        
        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // base.BuildContextualMenu(evt);
        {
            var types = TypeCache.GetTypesDerivedFrom<ActionNode>();

            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}",(a)=>CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();

            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}",(a)=>CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();

            foreach (var type in types)
            {
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}",(a)=>CreateNode(type));
            }
        }
    }

    private void CreateNode(Type type)
    {
        MNode node = tree.CreateNode(type);
        CreateNodeView(node);
    }

    void CreateNodeView(MNode node)
    {
        NodeView nodeView = new NodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }
}