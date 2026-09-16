using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;
using Xenon.NodeSystem;

namespace Editor.ProjectSystem
{
    public class ImTreeNode
    {
        public List<ImTreeNode> Children = new List<ImTreeNode>();
        public int id = 0;
        public string name = "";

        public Node ReferenceNode;

        public void CascadeAction(Action<ImTreeNode> callable)
        {
            callable(this);
            foreach (ImTreeNode child in Children)
            {
                child.CascadeAction(callable);
            }
        }

        public void Render()
        {
            if (ImGui.TreeNode(name))
            {
                RenderExtras();
                foreach (ImTreeNode child in Children)
                {
                    child.Render();
                }
                ImGui.TreePop();
            }
        }

        public virtual void RenderExtras()
        {

        }
    }
}
