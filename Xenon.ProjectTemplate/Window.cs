using ImGuiNET;
using Microsoft.Win32;
using NeoVeldrid.StartupUtilities;
using Xenon;
using Xenon.Context;
using Xenon.ImGuiCtx;

namespace ProjectTemplate
{
    public partial class Window : XenonWindow
    {
        private static bool _showFilePicker = false;
        private static ImGuiFilePicker _filePicker = new ImGuiFilePicker();


        public static int FilePickerMode = 0;
        public static void Main(string[] args)
        {

            // Create the SDL2 Window CreateInfo
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1366,
                WindowHeight = 768,
                WindowTitle = "Project - Xenon"
            };

            // Create the window
            Globals.program.CreateWindow(args, windowCI);

            // Create GPU resources, you can override this to modify or hook into it
            Globals.program.CreateResources();

            // register types, you can also override this
            Globals.program.RegisterTypes();

            // Change scene to an embedded file, you can load paths like normal
            // But prefix with 'emb.' to load embedded resources

            // Start the render loop, this WILL take over the main thread, be warned
            Globals.program.StartRenderLoop();

            // Dispose all resources. This will not run until RenderLoop is broken, by the window being closed
            Globals.program.DisposeResources();
        }

        public override void ImGuiLifecycle(float deltaTime, RenderContext ctx)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open Scene"))
                    {
                        _filePicker.SetFilter("*.xscn");
                        _filePicker.Refresh();
                        _showFilePicker = true;
                        FilePickerMode = 24;
                    }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit")) ctx.Window.Close();
                    ImGui.EndMenu();
                }

                string text = $"FPS: {1f / deltaTime:0.0}";

                float tw = ImGui.CalcTextSize(text).X;
                float padding = ImGui.GetStyle().ItemSpacing.X;

                ImGui.SameLine(ImGui.GetWindowWidth() - tw - padding);
                ImGui.TextUnformatted(text);
    
                ImGui.EndMainMenuBar();
            }
            HandleFilePicker();
            
        }

        public static void HandleFilePicker()
        {
            if (_showFilePicker)
            {
                if (_filePicker.Draw(ref _showFilePicker))
                {
                    string finalChosenFile = _filePicker.SelectedPath;
                    switch (FilePickerMode)
                    {
                        case 0:
                            break;
                        case 24:
                            Globals.program.ChangeSceneToFile(finalChosenFile);
                            break;

                    }
                }
            }
        }

        public static void Window_Scenes(float deltaTime, RenderContext ctx)
        {
            ImGui.Begin("Scenes");
            if (ImGui.Button("Main"))
            {
                Globals.program.ChangeSceneToFile("emb.ProjectTemplate.Resources.main.xscn");
            }
            ImGui.End();

        }
    }


}