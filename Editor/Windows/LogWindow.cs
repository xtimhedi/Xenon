using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xenon.Context;

namespace Editor.Windows
{
    public class LogWindow : EditorWindow
    {
        private bool _scrollToBottom = true;
        public LogWindow(string title) : base(title)
        {
        }

        public override void Window(double dt, RenderContext ctx)
        {
            ImGui.Begin(Title);
            ImGui.Checkbox("Auto-Scroll", ref _scrollToBottom);

            ImGui.Separator();

            if (ImGui.BeginChild("LSR", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));

                for (int i = 0; i < Xenon.XEN.Logger.LogFile.Count; i++)
                {
                    ImGui.TextUnformatted(Xenon.XEN.Logger.LogFile[i]);

                }

                if (_scrollToBottom)
                {
                    ImGui.SetScrollHereY(1.0f);

                }

                ImGui.PopStyleVar();
            }

            ImGui.EndChild();


            ImGui.End();
        }
    }
}
