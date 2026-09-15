using Editor;
using Editor.ProjectSystem;
using ImGuiColorTextEditNet;
using ImGuiNET;
using NeoVeldrid.StartupUtilities;
using System.Numerics;
using Xenon;
using Xenon.Context;
using Xenon.ImGuiCtx;
using Xenon.NodeSystem;
public class Program : XenonWindow
{

    public static EditorCamera3D camera;
    private static bool _showFilePicker = false;
    private static ImGuiFilePicker _filePicker = new ImGuiFilePicker();

    public List<EditorWindow> Windows = new List<EditorWindow>();

    public void NewWindow(EditorWindow wind)
    {
        Windows.Add(wind);
    }
    public void DestroyWindow(EditorWindow wind)
    {
        Windows.Remove(wind);
    }

    public static int FilePickerMode = 0;

    public static void Main(string[] args)
    {
        Globals.program.NewWindow(new Editor.Windows.TreeWindow("Tree"));
        Globals.program.NewWindow(new Editor.Windows.NullWindow("Filesystem"));
        Globals.program.NewWindow(new Editor.Windows.NullWindow("2"));
        Globals.program.NewWindow(new Editor.Windows.NullWindow("3"));
        Globals.program.NewWindow(new Editor.Windows.LogWindow("Engine Log"));

        // Create the SDL2 Window CreateInfo
        WindowCreateInfo windowCI = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 1366,
            WindowHeight = 768,
            WindowTitle = "XenonEditor"
        };

        // Create the window
        Globals.program.CreateWindow(args, windowCI);

        XEN.Logger.Log(Templates.ProjFileTemplate.template);
        // Create GPU resources, you can override this to modify or hook into it
        Globals.program.CreateResources();

        // register types, you can also override this
        Globals.program.RegisterTypes();

        // Change scene to an embedded file, you can load paths like normal
        // But prefix with 'emb.' to load embedded resources
        Globals.program.ChangeSceneToFile("emb.Editor.EmptyScene.xscn");

        // Start the render loop, this WILL take over the main thread, be warned
        Globals.program.StartRenderLoop();

        // Dispose all resources. This will not run until RenderLoop is broken, by the window being closed
        Globals.program.DisposeResources();
    }
    public TextEditor editor = new TextEditor();

    public static int MODE_3D = 0;
    public static int MODE_TEXTEDIT = 1;

    public string[] items = new[]
    {
        "3D",
        "Scripting"
    };

    public static int Mode = 0;

    public override void ImGuiLifecycle(float deltaTime, RenderContext ctx)
    {
        // 1. Draw Menu Bar FIRST so ImGui updates the viewport WorkSize/WorkPos
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
                if (ImGui.MenuItem("Exit"))
                {
                    ctx.Window.Close();
                }
                ImGui.EndMenu();
            }

                ImGui.Combo("", ref Mode, items, items.Length);



            string text = $"FPS: {1f / deltaTime:0.0}";
            float tw = ImGui.CalcTextSize(text).X;
            float padding = ImGui.GetStyle().ItemSpacing.X;
            ImGui.SameLine(ImGui.GetWindowWidth() - tw - padding);
            ImGui.TextUnformatted(text);

            ImGui.EndMainMenuBar();
        }


        // 2. NOW create the dockspace, correctly offset by the menu bar
        ExplicitDockSpace();

        foreach (EditorWindow window in Windows)
        {
            window.Window(deltaTime, ctx);
        }

        if (Mode == MODE_TEXTEDIT)
        {
            ImGui.Begin("Text Editor", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            editor.Render("Script Editor");
            ImGui.End();
        }
    }


    

    public static void ExplicitDockSpace()
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking |
                                      ImGuiWindowFlags.NoTitleBar |
                                      ImGuiWindowFlags.NoCollapse |
                                      ImGuiWindowFlags.NoResize |
                                      ImGuiWindowFlags.NoMove |
                                      ImGuiWindowFlags.NoBringToFrontOnFocus |
                                      ImGuiWindowFlags.NoNavFocus |
                                      ImGuiWindowFlags.NoBackground;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.Begin("XenonMasterDockSpace", windowFlags);
        ImGui.PopStyleVar();

        // Explicitly generate a valid, non-zero ID
        uint dockspaceId = ImGui.GetID("XenonCentralDockSpace");

        // Pass the explicit ID and the Passthru flag
        ImGui.DockSpace(dockspaceId, new Vector2(0.0f, 0.0f), ImGuiDockNodeFlags.PassthruCentralNode);

        ImGui.End();
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

    public override void BuildExternalTree(Node treeRoot)
    {
        camera = new EditorCamera3D();
        treeRoot.AddChild(camera);

        // Position the camera slightly up and back
        camera.Position = new Vector3(0f, 5f, 10f);

        // Convert degrees to radians! 
        // Pitch = -30 degrees (tilt down), Yaw = 0 degrees
        float pitch = -30f * (MathF.PI / 180f);
        float yaw = 0f * (MathF.PI / 180f);
        float roll = 0f * (MathF.PI / 180f);

        camera.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

        GridNode3D grid = new GridNode3D();
        treeRoot.AddChild(grid);
    }


}