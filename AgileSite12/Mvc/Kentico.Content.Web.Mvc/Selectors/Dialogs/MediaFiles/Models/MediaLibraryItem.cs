using System;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Represents an item for media library selector.
    /// </summary>
    internal class MediaLibraryItem
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("createFile")]
        public bool CreateFile { get; set; }
    }
}
