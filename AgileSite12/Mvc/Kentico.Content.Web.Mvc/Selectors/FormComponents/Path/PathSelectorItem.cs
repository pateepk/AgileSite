using System;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents an item for a path selector.
    /// </summary>
    public class PathSelectorItem
    {
        /// <summary>
        /// Node Alias Path of a page.
        /// </summary>
        [JsonProperty("nodeAliasPath")]
        public string NodeAliasPath { get; set; }
    }
}
