using System.Runtime.Serialization;
using System;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the configuration of a form component within a <see cref="ZoneConfiguration"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "FormComponent")]
    public sealed class FormComponentConfiguration
    {
        private Guid? mIdentifier;


        /// <summary>
        /// Gets the identifier of the form component instance.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier
        {
            get
            {
                return Properties?.Guid ?? mIdentifier ?? throw new InvalidOperationException("Cannot retrieve identifier of a form component configuration which is missing its properties.");
            }
            private set
            {
                mIdentifier = value;
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the form component definition.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Gets or sets the form component properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public FormComponentProperties Properties { get; set; }
    }
}
