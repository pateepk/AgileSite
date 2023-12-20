using System.Collections.Generic;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Metadata describing Form builder components for the client.
    /// </summary>
    public sealed class FormComponentsMetadata
    {
        /// <summary>
        /// List of form components metadata.
        /// </summary>
        [JsonProperty("formComponents", TypeNameHandling = TypeNameHandling.None)]
        public IList<FormComponentMetadata> FormComponents { get; internal set; }


        /// <summary>
        /// Returns sections metadata used in form builder.
        /// </summary>
        [JsonProperty("sections", TypeNameHandling = TypeNameHandling.None)]
        public IList<SectionMetadata> Sections { get; internal set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentsMetadata"/> class.
        /// </summary>
        public FormComponentsMetadata()
        {
            FormComponents = new List<FormComponentMetadata>();
            Sections = new List<SectionMetadata>();
        }
    }
}
