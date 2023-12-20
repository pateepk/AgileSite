using System.Collections.Generic;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Represents a node for media files selector folder tree.
    /// </summary>
    internal class MediaLibraryFolderNode
    {
        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("children")]
        public List<MediaLibraryFolderNode> Children { get; set; }


        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
