using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Xenon.ContentSystem.Classes;

namespace Xenon.ContentSystem
{
    public class SceneTreeLoader
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public XenonScene LoadScene(string path)
        {
            bool isEmbedded = path.StartsWith("emb.", StringComparison.OrdinalIgnoreCase) ||
                             path.StartsWith("emb/", StringComparison.OrdinalIgnoreCase);

            if (isEmbedded)
            {
                string manifestName = path.Substring(4);

                // Checks entry assembly first (TestGame.exe), fallback to executing assembly (Xenon.dll)
                Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream(manifestName);

                if (stream == null)
                {
                    XEN.Logger.LogError($"Embedded scene resource missing: '{manifestName}' in assembly '{assembly.FullName}'", "SceneLoader");
                    return null;
                }

                return LoadScene(stream, manifestName);
            }

            if (!File.Exists(path))
            {
                XEN.Logger.LogError($"Scene file missing at path: {Path.GetFullPath(path)}", "SceneLoader");
                return null;
            }

            try
            {
                using Stream stream = File.OpenRead(path);
                return LoadScene(stream, path);
            }
            catch (Exception ex)
            {
                XEN.Logger.LogError($"Exception opening scene file '{path}': {ex.Message}", "SceneLoader");
                return null;
            }
        }

        public XenonScene LoadScene(Stream stream, string identifier = "Stream")
        {
            try
            {
                XenonScene scene = JsonSerializer.Deserialize<XenonScene>(stream, _jsonOptions);

                if (scene == null)
                {
                    XEN.Logger.LogError($"Failed to deserialize scene from: {identifier}", "SceneLoader");
                    return null;
                }

                XEN.Logger.Log($"Successfully loaded scene: {identifier}", "SceneLoader");
                return scene;
            }
            catch (Exception ex)
            {
                XEN.Logger.LogError($"Exception parsing scene JSON '{identifier}': {ex.Message}", "SceneLoader");
                return null;
            }
        }
    }
}