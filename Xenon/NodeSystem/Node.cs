using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Xenon.Context;

namespace Xenon.NodeSystem
{
    public class Node
    {
        public string Name { get; set; } = "Node";
        public Node Parent { get; private set; }
        public List<Node> Children { get; } = new List<Node>();

        public void AddChild(Node child)
        {
            child.Parent?.Children.Remove(child);
            child.Parent = this;
            Children.Add(child);
        }

        public virtual void Ready(RenderContext context)
        {
            foreach (var child in Children)
                child.Ready(context);
        }

        public virtual void Update(float deltaTime)
        {
            foreach (var child in Children)
                child.Update(deltaTime);
        }

        public virtual void Draw(RenderContext context)
        {
            foreach (var child in Children)
                child.Draw(context);
        }

        public virtual void Input(InputSnapshot snapshot)
        {
            foreach (var child in Children)
                child.Input(snapshot);
        }

        public void QueueFree()
        {
            Parent.Children.Remove(this);
        }
    }

    

    

    
}