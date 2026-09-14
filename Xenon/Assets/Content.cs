using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Xenon.Assets
{
    public static class Content
    {
        private static readonly Dictionary<string, object> _cache = new();
        private static readonly Dictionary<string, IAssetLoader> _loaders = new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterLoader(string extension, IAssetLoader loader)
        {
            _loaders[extension] = loader;
            XEN.Logger.Log($"Registered file extension '{extension}' for type {loader.GetType()}", "Content");
        }

        public static T Load<T>(string path)
        {
            bool isEmbedded = path.StartsWith("emb.", StringComparison.OrdinalIgnoreCase) ||
                             path.StartsWith("emb/", StringComparison.OrdinalIgnoreCase);

            string cacheKey = isEmbedded ? $"emb.{path.Substring(4)}" : Path.GetFullPath(path);

            if (_cache.TryGetValue(cacheKey, out object cachedAsset))
            {
                return (T)cachedAsset;
            }

            string extension = Path.GetExtension(path);
            if (!_loaders.TryGetValue(extension, out IAssetLoader loader))
            {
                XEN.Logger.LogError($"No loader registered for extension '{extension}'", "Content");
                throw new KeyNotFoundException($"No loader registered for extension '{extension}'");
            }

            object loadedObject;

            if (isEmbedded)
            {
                string manifestName = path.Substring(4);

                using Stream stream = GetResourceStream(manifestName, out Assembly foundAssembly);

                if (stream == null)
                {
                    XEN.Logger.LogError($"Embedded resource '{manifestName}' not found in entry or executing assemblies.", "Content");
                    throw new FileNotFoundException($"Embedded resource '{manifestName}' not found.");
                }

                loadedObject = loader.Load(stream);
            }
            else
            {
                loadedObject = loader.Load(cacheKey);
            }

            if (loadedObject is not T typedAsset)
            {
                throw new InvalidCastException($"Asset at '{path}' loaded as {loadedObject.GetType().Name}, but {typeof(T).Name} was expected.");
            }

            _cache[cacheKey] = typedAsset;
            return typedAsset;
        }

        private static Stream GetResourceStream(string manifestName, out Assembly assembly)
        {
            // 1. Try entry assembly (TestGame.exe)
            assembly = Assembly.GetEntryAssembly();
            Stream stream = assembly?.GetManifestResourceStream(manifestName);

            if (stream != null) return stream;

            // 2. Fall back to executing assembly (Xenon.dll)
            assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(manifestName);
        }

        public static void Unload(string path)
        {
            string cacheKey = path.StartsWith("emb.", StringComparison.OrdinalIgnoreCase) ||
                             path.StartsWith("emb/", StringComparison.OrdinalIgnoreCase)
                ? $"emb.{path.Substring(4)}"
                : Path.GetFullPath(path);

            if (_cache.Remove(cacheKey, out object asset))
            {
                if (asset is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public static void UnloadAll()
        {
            foreach (object asset in _cache.Values)
            {
                if (asset is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _cache.Clear();
        }
    }
}