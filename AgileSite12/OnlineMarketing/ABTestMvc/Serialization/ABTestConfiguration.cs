using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Represents the configuration of a AB test of a given document.
    /// </summary>
    [DataContract(Namespace = "", Name = "configuration")]
    public sealed class ABTestConfiguration
    {
        /// <summary>
        /// Collection of AB test variants associated with given document.
        /// </summary>
        [DataMember]
        [JsonProperty("variants", Required = Required.Always)]
        public ICollection<ABTestVariant> Variants { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ABTestConfiguration"/> class.
        /// </summary>
        public ABTestConfiguration()
        {
            Variants = new List<ABTestVariant>();
        }
    }
}
