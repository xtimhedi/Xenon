using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Context;
using ImGuiNET;

namespace Editor.Windows
{
    public class PropertiesWindow : EditorWindow
    {
        private string _tl1 = "";
        public PropertiesWindow(string title) : base(title)
        {
        }

        public override void Window(double dt, RenderContext ctx)
        {
            ImGui.Begin(Title);

            // top section
            
            ImGui.SameLine();
            ImGui.InputText("##LoadNodeID", ref _tl1, 32);
            ImGui.SameLine();
            if (ImGui.Button("Load"))
            {
            
            }
            ImGui.Separator();
            

            ImGui.End();
        }
    }
}
