using System.Diagnostics;
using System.Numerics;
using System.Text;
using NeoVeldrid;
using NeoVeldrid.Sdl2;
using NeoVeldrid.SPIRV;
using NeoVeldrid.StartupUtilities;
using ImGuiNET;      // Added
using Xenon.Assets;
using Xenon.ContentSystem;
using Xenon.ContentSystem.Classes;
using Xenon.Context;
using Xenon.NodeSystem;

namespace Xenon
{
    public class XenonWindow
    {
        private static GraphicsDevice _graphicsDevice;
        private static CommandList _commandList;
        private static Pipeline _pipeline;
        private static Pipeline _pipeline2D;
        private static ResourceLayout _matrixLayout;
        private static Shader[] _shaders;

        // Added ImGuiRenderer
        private static Xenon.ImGuiCtx.ImGuiRenderer _imGuiRenderer;

        public static RenderContext _renderContext;
        private static Node _sceneRoot;

        private static SceneTreeLoader TreeLoader = new SceneTreeLoader();
        private static SceneTreeSaver TreeSaver = new SceneTreeSaver();

        public static XenonScene CurrentScene = new XenonScene();

        public static EngineConfig EC = new EngineConfig
        {
            TargetFramerate = 144.0d,
            VsyncEnable = false
        };

        private const string VertexCode = @"
#version 450

layout(set = 0, binding = 0) uniform FrameUniforms
{
    mat4 MVP;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 UV;

layout(location = 0) out vec4 fsin_Color;
layout(location = 1) out vec2 fsin_UV;

void main()
{
    gl_Position = MVP * vec4(Position, 1.0);
    fsin_Color = Color;
    fsin_UV = UV;
}";

        private const string FragmentCode = @"
#version 450

layout(set = 0, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 0, binding = 2) uniform sampler SurfaceSampler;

layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_UV;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_UV) * fsin_Color;
}";

        public void CreateResources()
        {
            XEN.Logger.Log("Created ResourceFactory", "StartupFactory");
            ResourceFactory factory = _graphicsDevice.ResourceFactory;

            XEN.Logger.Log("Created matrixLayout", "StartupFactory");

            _matrixLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("FrameUniforms", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("SurfaceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SurfaceSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            XEN.Logger.Log("Created vertexLayout", "StartupFactory");

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("UV", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            );
            XEN.Logger.Log("Created shader", "StartupFactory");

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main");

            _shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            XEN.Logger.Log("Created pipeline", "StartupFactory");

            // 1. Standard 3D Pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.Back,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: true,
                    scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { _matrixLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new[] { vertexLayout },
                    shaders: _shaders),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            // 2. Dedicated 2D Overlay Pipeline
            GraphicsPipelineDescription pipeline2DDescription = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = new RasterizerStateDescription(
                    cullMode: FaceCullMode.None,
                    fillMode: PolygonFillMode.Solid,
                    frontFace: FrontFace.CounterClockwise,
                    depthClipEnabled: false,
                    scissorTestEnabled: false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { _matrixLayout },
                ShaderSet = new ShaderSetDescription(
                    vertexLayouts: new[] { vertexLayout },
                    shaders: _shaders),
                Outputs = _graphicsDevice.SwapchainFramebuffer.OutputDescription
            };

            _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);
            _pipeline2D = factory.CreateGraphicsPipeline(pipeline2DDescription);
            _commandList = factory.CreateCommandList();

            _imGuiRenderer = new Xenon.ImGuiCtx.ImGuiRenderer(
                _graphicsDevice,
                _graphicsDevice.SwapchainFramebuffer.OutputDescription,
                window.Width,
                window.Height
            );

            XEN.Logger.Log("Context finished", "StartupFactory");

            _renderContext = new RenderContext
            {
                Device = _graphicsDevice,
                CommandList = _commandList,
                Pipeline = _pipeline,
                Pipeline2D = _pipeline2D,
                MatrixLayout = _matrixLayout,
            };

            CreateDefaultTexture(factory);
        }

        public static void LimitFrameRate(Stopwatch frameStopwatch, double targetFps, ref double lastFrameTime)
        {
            double targetFrameTime = 1.0 / targetFps;

            while (true)
            {
                double currentTime = frameStopwatch.Elapsed.TotalSeconds;
                double elapsedTime = currentTime - lastFrameTime;

                if (elapsedTime >= targetFrameTime)
                {
                    lastFrameTime = currentTime; // Update the reference timestamp for the next frame
                    break;
                }

                // Calculate time remaining in milliseconds
                double remainingMs = (targetFrameTime - elapsedTime) * 1000.0;

                // Yield CPU control if there is a significant chunk of time left (> 1ms)
                if (remainingMs > 1.0)
                {
                    Thread.Sleep((int)(remainingMs - 0.5)); // Leave a tiny buffer to account for OS timer inaccuracy
                }
                else
                {
                    Thread.Yield(); // Spin lightly for sub-millisecond precision
                }
            }
        }

        private void CreateDefaultTexture(ResourceFactory factory)
        {
            TextureDescription desc = TextureDescription.Texture2D(
                width: 2,
                height: 2,
                mipLevels: 1,
                arrayLayers: 1,
                format: PixelFormat.R8_G8_B8_A8_UNorm,
                usage: TextureUsage.Sampled
            );

            Texture defaultTex = factory.CreateTexture(desc);

            byte[] pixels = new byte[]
            {
                255,   0, 255, 255,
                 30,  30,  30, 255,
                 30,  30,  30, 255,
                255,   0, 255, 255
            };

            _graphicsDevice.UpdateTexture(
                defaultTex,
                pixels,
                0, 0, 0,
                2, 2, 1,
                0, 0
            );

            _renderContext.DefaultTexture = defaultTex;
            _renderContext.DefaultTextureView = factory.CreateTextureView(defaultTex);
        }

        public void BuildSceneTree()
        {
            XEN.Logger.Log("Loading SceneTree from file", "SceneBuilder");

            if (CurrentScene != null)
            {
                _sceneRoot = CurrentScene.Instantiate();
            }

            if (_sceneRoot == null)
            {
                XEN.Logger.LogWarn("No scene set. Creating default root node.", "SceneBuilder");
                _sceneRoot = new Node { Name = "Root" };
            }
            BuildExternalTree(_sceneRoot);

            _sceneRoot.Ready(_renderContext);
        }

        public void ChangeSceneToFile(string filename)
        {
            XEN.Logger.Log($"Changing scene to file {filename}");
            CurrentScene = TreeLoader.LoadScene(filename);
            BuildSceneTree();
        }

        private Sdl2Window window;

        public void CreateWindow(string[] args, WindowCreateInfo windowCI)
        {
            Xenon.ContentSystem.WindowsAnsiInitializer.EnsureEnabled();
            XEN.Logger.Log("Xenon 3.0.1b_ShippingDebug - Copyright 2026 Newertech LLC and Xenon contributors | https://xenon3d.xyz", "Engine");
            XEN.Logger.Log($"StartPosition: {windowCI.X}, {windowCI.Y}", "WindowInfo");
            XEN.Logger.Log($"Size: {windowCI.WindowWidth}, {windowCI.WindowHeight}", "WindowInfo");
            XEN.Logger.Log($"Title: {windowCI.WindowTitle}", "WindowInfo");

            XEN.Logger.Log("Created Window", "Engine");
            window = NeoVeldridStartup.CreateWindow(ref windowCI);
            XEN.Logger.Log("Context started", "SDL2");
            GraphicsDeviceOptions options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
                SwapchainDepthFormat = PixelFormat.R16_UNorm
            };
            _graphicsDevice = NeoVeldridStartup.CreateGraphicsDevice(window, options);
            XEN.Logger.Log("Created GPU Context", "Engine");

            // Added: Forward Window Resizes to ImGui and Veldrid Swapchain
            window.Resized += () =>
            {
                _graphicsDevice.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                _imGuiRenderer?.WindowResized(window.Width, window.Height);
            };
            Xenon.Audio.AudioSystem.Initialize();
            XEN.Logger.Log("Created audio context", "Audio");
        }

        public void RegisterTypes()
        {
            Content.RegisterLoader(".png", new TextureLoader(_graphicsDevice));
            Content.RegisterLoader(".jpg", new TextureLoader(_graphicsDevice));
            Content.RegisterLoader(".jpeg", new TextureLoader(_graphicsDevice));
            Content.RegisterLoader(".xmdl", new ObjMeshLoader());
            Content.RegisterLoader(".wav", new AudioClipLoader());
            Content.RegisterLoader(".mp3", new Mp3ClipLoader());
            XEN.Logger.Log("Registered types", "Content");
        }

        public virtual void ImGuiLifecycle(float deltaTime, RenderContext ctx)
        {
            
        }

        public virtual void BuildExternalTree(Node treeRoot)
        {

        }

        public static bool hasInited = false;

        public void StartRenderLoop()
        {
            BuildSceneTree();
            _renderContext.Window = window;
            Stopwatch stopwatch = Stopwatch.StartNew();
            double previousTime = 0;
            int frame = 0;
            XEN.Logger.Log("Started State1", "RenderStartup");

            Sprite2D NoSceneSprite = new Sprite2D
            {
                Texture = Content.Load<Texture>("emb.Xenon.Marketing.noScene.png"),
                Position = new Vector2(_renderContext.Window.Width / 2, _renderContext.Window.Height / 2),
                Scale = new Vector2(300, 26)
            };

            TitleSprite logoSprite = new TitleSprite
            {
                Texture = Assets.Content.Load<Texture>("emb.Xenon.Marketing.xenon_poweredBy.png"),
                Position = new Vector2(_renderContext.Window.Width / 2, _renderContext.Window.Height / 2),
                Scale = new Vector2(320, 143)
            };
            _sceneRoot.AddChild(logoSprite);


            Stopwatch frameStopwatch = Stopwatch.StartNew();
            double lastFrameTime = 0.0;
            double maxFps = 60.0; // Change this to your desired limit dynamically


            while (window.Exists)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                float deltaTime = (float)(currentTime - previousTime);
                previousTime = currentTime;

                NeoVeldrid.InputSnapshot ss = window.PumpEvents();

                // Added: Feed input to ImGui
                _imGuiRenderer.Update(deltaTime, ss);

                // ---> Example ImGui Window (Remove or modify as needed) <---
                ImGuiLifecycle(deltaTime, _renderContext);

                if (frame == 5000)
                {
                    XEN.Logger.Log("Started State2", "RenderStartup");

                    logoSprite.QueueFree();
                    logoSprite = new TitleSprite
                    {
                        Texture = Assets.Content.Load<Texture>("emb.Xenon.Marketing.veldrid-logo-horizontal.png"),
                        Position = new Vector2(_renderContext.Window.Width / 2, _renderContext.Window.Height / 2),
                        Scale = new Vector2(400, 151)
                    };
                    _sceneRoot.AddChild(logoSprite);
                }
                else if (frame == 10000)
                {
                    XEN.Logger.Log("Started State3", "RenderStartup");

                    logoSprite.QueueFree();
                    logoSprite = new TitleSprite
                    {
                        Texture = Assets.Content.Load<Texture>("emb.Xenon.Marketing.vk.png"),
                        Position = new Vector2(_renderContext.Window.Width / 2, _renderContext.Window.Height / 2),
                        Scale = new Vector2(400, 106)
                    };
                    _sceneRoot.AddChild(logoSprite);
                }
                else if (frame == 15000)
                {
                    logoSprite.QueueFree();
                    XEN.Logger.Log("RenderSetup complete!", "RenderStartup");
                    hasInited = true;

                }

                // 1. Process Updates
                _sceneRoot.Update(deltaTime);
                CS.CollisionSystem.ProcessCollisions();
                _sceneRoot.Input(ss); // (Note: You may want to early-return here if ImGui.GetIO().WantCaptureMouse is true)

                // 2. Render Hierarchy
                _commandList.Begin();
                _commandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
                _commandList.ClearColorTarget(0, RgbaFloat.Black);
                _commandList.ClearDepthStencil(1f);

                // dp1: 3D
                if (frame > 15000)
                {
                    DrawNodesOfType<Node3D>(_sceneRoot, _renderContext);
                    _commandList.ClearDepthStencil(1f);

                    // dp2: 2D
                    DrawNodesOfType<Node2D>(_sceneRoot, _renderContext);
                }
                else
                {
                    DrawNodesOfType<TitleSprite>(_sceneRoot, _renderContext);
                }

                // Added: Render ImGui data onto the CommandList right before submission
                _imGuiRenderer.Render(_graphicsDevice, _commandList);

                _commandList.End();
                _graphicsDevice.SubmitCommands(_commandList);
                _graphicsDevice.SwapBuffers();
                frame++;
                LimitFrameRate(frameStopwatch, hasInited ? EC.TargetFramerate : 80000, ref lastFrameTime);
                // by the way rev.
                // why would we want the framerate in a seperate EC/EngineConfig struct?
                // i understand magic numbers are like bad or smth, but we dont need to modify this EVER, the engine is to remain locked at 144 FPS
            }
        }

        private void DrawNodesOfType<T>(Node parent, RenderContext context) where T : Node
        {
            if (parent is T targetNode)
            {
                targetNode.Draw(context);
            }

            foreach (var child in parent.Children)
            {
                DrawNodesOfType<T>(child, context);
            }
        }


        // ethan... for the love of god, set these log messages in the respective dispose calls, not here! - Xtimhedi
        public void DisposeResources()
        {
            XEN.Logger.Log("Preparing to close", "Shutdowm");
            Xenon.Audio.AudioSystem.Shutdown();
            XEN.Logger.Log("Waiting for GPU to stop rendering and go idle", "Shutdowm");
            _graphicsDevice.WaitForIdle();
            // Added: Dispose ImGui
            XEN.Logger.Log("Disposing ImGui renderer", "Shutdowm");
            _imGuiRenderer.Dispose();
            XEN.Logger.Log("Disposing matrix", "Shutdowm");
            _matrixLayout.Dispose();
            XEN.Logger.Log("Disposing rendering pipeline 3D", "Shutdowm");
            _pipeline.Dispose();
            XEN.Logger.Log("Disposing rendering pipeline 2D", "Shutdowm");
            _pipeline2D.Dispose();
            XEN.Logger.Log("Ending Command List", "Shutdowm");
            _commandList.Dispose();
            XEN.Logger.Log("Freeing graphics resources", "Shutdowm");
            _graphicsDevice.Dispose();
            XEN.Logger.Log("Done cleaning up, closing!", "Shutdowm");

        }
    }
}