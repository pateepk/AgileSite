using System;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents item for media files selector.
    /// </summary>
    public class MediaFilesSelectorItem
    {
        /// <summary>
        /// Media file GUID.
        /// </summary>
        [JsonProperty("fileGuid")]
        public Guid FileGuid { get; set; }
    }
}
