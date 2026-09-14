using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan.Xlib;
using Xenon.Context;

namespace Xenon
{
    public static class ImGuiWindows
    {
        public static void Engine_DebugWindow(float deltaTime, RenderContext ctx)
        {
            ImGui.Begin("Engine Debug");
            ImGui.Text($"FPS: {1f / deltaTime:0.0}");
            if (ImGui.Button("Quit"))
            {
                ctx.Window.Close();
            }
            ImGui.End();
        }
    }
}
