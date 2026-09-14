using System;
using System.IO;
using NeoVeldrid;
using NeoVeldrid.ImageSharp;

namespace Xenon.Assets
{
    public interface IAssetLoader
    {
        object Load(string filePath);
        object Load(Stream stream);
    }

    public class ObjMeshLoader : IAssetLoader
    {
        public object Load(string filePath)
        {
            return Xenon.Context.Mesh.LoadFromObj(filePath, RgbaFloat.White);
        }

        public object Load(Stream stream)
        {
            // Assumes your Mesh class has a stream-based overload
            return Xenon.Context.Mesh.LoadFromObj(stream, RgbaFloat.White);
        }
    }

    public class TextureLoader : IAssetLoader
    {
        private readonly GraphicsDevice _graphicsDevice;

        public TextureLoader(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        }

        public object Load(string filePath)
        {
            ImageSharpTexture imageTexture = new ImageSharpTexture(filePath, mipmap: true);
            return imageTexture.CreateDeviceTexture(_graphicsDevice, _graphicsDevice.ResourceFactory);
        }

        public object Load(Stream stream)
        {
            // Veldrid.ImageSharp natively supports loading directly from a Stream
            ImageSharpTexture imageTexture = new ImageSharpTexture(stream, mipmap: true);
            return imageTexture.CreateDeviceTexture(_graphicsDevice, _graphicsDevice.ResourceFactory);
        }
    }
}