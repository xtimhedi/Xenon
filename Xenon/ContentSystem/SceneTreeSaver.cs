using System;
using System.IO;
using System.Text.Json;
using Xenon.ContentSystem.Classes;

namespace Xenon.ContentSystem
{
    public class SceneTreeSaver
    {
        private static readonly JsonSerializerOptions _saveOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public void SaveScene(string scenePath, XenonScene scene)
        {
            try
            {
                string data = JsonSerializer.Serialize(scene, _saveOptions);
                File.WriteAllText(scenePath, data);
                XEN.Logger.Log($"Successfully saved scene to '{scenePath}'.", "SceneSaver");
            }
            catch (Exception ex)
            {
                XEN.Logger.LogError($" Failed to save scene '{scenePath}': {ex.Message}", "SceneSaver");
            }
        }
    }
}