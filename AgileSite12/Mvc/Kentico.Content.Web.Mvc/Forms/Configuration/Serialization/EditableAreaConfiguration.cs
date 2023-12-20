using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the editable area configuration within a <see cref="FormBuilderConfiguration"/> instance.
    /// </summary>
    public sealed class EditableAreaConfiguration
    {
        /// <summary>
        /// Name of the default editable area used in the Form builder.
        /// </summary>
        public const string IDENTIFIER = "DefaultFormBuilderArea";


        /// <summary>
        /// Identifier of the editable area.
        /// </summary>
        /// <remarks>
        /// Form builder contains only one default editable area, therefore, <see cref="IDENTIFIER"/> is always returned.
        /// </remarks>
        [DataMember]
        [JsonProperty("identifier")]
        public string Identifier => IDENTIFIER;


        /// <summary>
        /// Sections within the editable area.
        /// </summary>
        [DataMember]
        [JsonProperty("sections")]
        public List<SectionConfiguration> Sections { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EditableAreaConfiguration"/> class.
        /// </summary>
        public EditableAreaConfiguration()
        {
            Sections = new List<SectionConfiguration>();
        }
    }
}
