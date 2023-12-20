using System.Collections.Generic;
using System.Runtime.Serialization;

using CMS.DocumentEngine;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents configuration of editable areas, sections, widget zones and widgets within the <see cref="TreeNode"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "Configuration")]
    public sealed class EditableAreasConfiguration
    {
        /// <summary>
        /// Editable areas within the page.
        /// </summary>
        [DataMember]
        [JsonProperty("editableAreas")]
        public List<EditableAreaConfiguration> EditableAreas { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public EditableAreasConfiguration()
        {
            EditableAreas = new List<EditableAreaConfiguration>();
        }
    }
}
