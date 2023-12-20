using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the configuration of a Form builder instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "Configuration")]
    public sealed class FormBuilderConfiguration
    {
        /// <summary>
        /// Editable areas within the Form builder.
        /// </summary>
        [DataMember]
        [JsonProperty("editableAreas")]
        public List<EditableAreaConfiguration> EditableAreas { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormBuilderConfiguration"/> class.
        /// </summary>
        public FormBuilderConfiguration()
        {
            EditableAreas = new List<EditableAreaConfiguration>();
        }
    }
}
