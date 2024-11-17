using System;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BehaviourTree tree;

    void Start()
    {
        tree = tree.Clone();
    }

    private void Update()
    {
        tree.Update();
    }
}