using System.Collections.Generic;
using System.Runtime.Serialization;

using CMS.DocumentEngine;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents configuration of editable area within the <see cref="TreeNode"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "EditableArea")]
    public sealed class EditableAreaConfiguration
    {
        /// <summary>
        /// Identifier of the editable area.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        /// <summary>
        /// Sections within editable area.
        /// </summary>
        [DataMember]
        [JsonProperty("sections")]
        public List<SectionConfiguration> Sections { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public EditableAreaConfiguration()
        {
            Sections = new List<SectionConfiguration>();
        }
    }
}
