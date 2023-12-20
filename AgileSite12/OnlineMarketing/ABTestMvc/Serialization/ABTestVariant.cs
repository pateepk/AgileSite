using System;
using System.Runtime.Serialization;

using CMS.DocumentEngine.Internal;

using Newtonsoft.Json;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Represents single AB test variant.
    /// </summary>
    [DataContract(Namespace = "", Name = "variant")]
    public sealed class ABTestVariant : IABTestVariant
    {
        /// <summary>
        /// Identifier of a AB test variant.
        /// </summary>
        [DataMember]
        [JsonProperty("guid", Required = Required.Always)]
        public Guid Guid { get; set; }


        /// <summary>
        /// Name of a AB test variant.
        /// </summary>
        [DataMember]
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }


        /// <summary>
        /// Indicates whether this AB test variant is original. In case of <see cref="IsOriginal"/> variant, data associated to this variant are loaded from respective properties of <see cref="DocumentCultureDataInfo"/>.
        /// </summary>
        [DataMember]
        [JsonProperty("isOriginal", Required = Required.Default)]
        public bool IsOriginal { get; set; }


        /// <summary>
        /// Configuration of page builder widgets in JSON format, associated with this variant.
        /// </summary>
        [DataMember]
        [JsonProperty("pageBuilderWidgets", Required = Required.Default)]
        public string PageBuilderWidgets { get; set; }


        /// <summary>
        /// Configuration of page template in JSON format, associated with this variant.
        /// </summary>
        [DataMember]
        [JsonProperty("pageTemplate", Required = Required.Default)]
        public string PageTemplate { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestVariant"/> class.
        /// </summary>
        /// <param name="name">Name of new variant.</param>
        /// <param name="isOriginal">Whether variant is original.</param>
        [JsonConstructor]
        public ABTestVariant(string name, bool isOriginal = false)
            : this(Guid.NewGuid(), name, isOriginal)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestVariant"/> class.
        /// </summary>
        /// <param name="guid">Guid of the variant.</param>
        /// <param name="name">Name of new variant.</param>
        /// <param name="isOriginal">Whether variant is original.</param>
        public ABTestVariant(Guid guid, string name, bool isOriginal = false)
        {
            Guid = guid;
            Name = name;
            IsOriginal = isOriginal;
        }
    }
}
