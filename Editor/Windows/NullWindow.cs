using System;
using System.Collections.Generic;
using System.Text;
using Xenon.Context;
using ImGuiNET;


namespace Editor.Windows
{
    public class NullWindow : EditorWindow
    {
        bool open = true;

        public NullWindow(string title) : base(title)
        {
        }

        public override void Window(double dt, RenderContext ctx)
        {
            if (open)
            {
                ImGui.Begin(Title, ref open);
                ImGui.SetWindowSize(new System.Numerics.Vector2(200, 200));
                ImGui.End();
            }
        }
    }
}
