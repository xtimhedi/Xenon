using Veldrid;
using Veldrid.Sdl2;
using Xenon.NodeSystem;

namespace Xenon.Context
{
    public class RenderContext
    {
        public GraphicsDevice Device { get; set; }
        public CommandList CommandList { get; set; }
        public Pipeline Pipeline { get; set; }
        public Pipeline Pipeline2D { get; set; }
        public ResourceLayout MatrixLayout { get; set; }
        public Camera3D CurrentCamera { get; set; }
        public Texture DefaultTexture { get; set; }
        public TextureView DefaultTextureView { get; set; }
        public Sdl2Window Window { get; set; }
    }
}