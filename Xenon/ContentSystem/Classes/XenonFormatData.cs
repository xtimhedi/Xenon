using System.Text.Json.Serialization;

namespace Xenon.ContentSystem.Classes
{
    public class XenonFormatData
    {
        [JsonPropertyName("xenon_version")]
        public string Xenon_Version { get; set; } = "";

        [JsonPropertyName("xenon_editor")]
        public string Xenon_Editor { get; set; } = "false";

        [JsonPropertyName("xenon_window_title")]
        public string Xenon_Window_Title { get; set; } = "";
    }
}