using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Context;
using ImGuiNET;
using Editor.ProjectSystem;
using Xenon.NodeSystem;

namespace Editor.Windows
{
    public class TreeWindow : EditorWindow
    {
        string NodeSearchText = "";
        public TreeWindow(string title) : base(title)
        {
        }

        public ImTreeNode root = new ImTreeNode()
        {
            name = "Scene"
        };

        int _id = 0;

        public void ParseTree(Node sourceSceneTree)
        {
            // Clear existing children to prevent duplication on refresh
            root.Children.Clear();

            // Walk through the source tree and build the UI tree
            foreach (Node childNode in sourceSceneTree.Children)
            {
                WalkTreee(childNode, root);
            }
        }

        public void WalkTreee(Node tree, ImTreeNode parent)
        {
            ImTreeNode n = new ImTreeNode
            {
                name = tree.Name,
                // Use a stable ID instead of random to preserve ImGui state
                id = _id,
                ReferenceNode = tree
            };
            _id++;

            parent.Children.Add(n);

            foreach (Node node in tree.Children)
            {
                WalkTreee(node, n);
            }
            
        }

        public override void Window(double dt, RenderContext ctx)
        {
            ImGui.Begin(Title);
            ImGui.SameLine();
            if (ImGui.Button("+"))
            {

            }
            ImGui.SameLine();
            if (ImGui.Button("ICS"))
            {

            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(90f);
            if (ImGui.InputText("##SceneSearcher", ref NodeSearchText, 24))
            {

            }
            ImGui.SameLine();
            if (ImGui.Button("Script"))
            {

            }
            ImGui.SameLine();
            if (ImGui.Button("Menu"))
            {   

            }

            root.Render();

            ImGui.End();
        }

        
    }
}
