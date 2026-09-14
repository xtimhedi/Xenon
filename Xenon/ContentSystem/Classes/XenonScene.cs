using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xenon.NodeSystem;

namespace Xenon.ContentSystem.Classes
{
    public class XenonScene
    {
        [JsonPropertyName("format")]
        public XenonFormatData FormatData { get; set; } = new();

        [JsonPropertyName("scene_data")]
        public List<XenonSceneNode> SceneData { get; set; } = new();

        /// <summary>
        /// Builds and returns the live runtime Node tree from this scene.
        /// </summary>
        public Node Instantiate()
        {
            if (SceneData == null || SceneData.Count == 0)
                return null;

            // Builds live runtime hierarchy starting at root
            return SceneData[0]?.BuildNode();
        }
    }
}