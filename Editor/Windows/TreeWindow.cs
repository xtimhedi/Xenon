using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Context;
using ImGuiNET;
using Editor.ProjectSystem;

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
            name = "Scene",
            Children = new List<ImTreeNode>
            {
                new ImTreeNode
                {
                    name = "test"
                }
            }
        };

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

        public void ParseTree()
        {

        }
    }
}
