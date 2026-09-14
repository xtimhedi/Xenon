using Veldrid.StartupUtilities;
using Xenon;

namespace TestGame
{
    public class Program : XenonWindow
    {
        public static void Main(string[] args)
        {
            Program program = new Program();
            WindowCreateInfo windowCI = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 3840,
                WindowHeight = 2160,
                WindowTitle = "TestGame - Xenon",
                WindowInitialState = Veldrid.WindowState.BorderlessFullScreen
            };
            program.CreateWindow(args, windowCI);
            program.CreateResources();
            program.RegisterTypes();
            program.ChangeSceneToFile("emb.TestGame.EmbeddedResources.test.xscn");
            program.StartRenderLoop();
            program.DisposeResources();
        }
    }
}