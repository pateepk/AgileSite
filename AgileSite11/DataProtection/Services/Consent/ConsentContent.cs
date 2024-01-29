using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.DataProtection
{
    /// <summary>
    /// Represents consent's language versions contained within the <see cref="ConsentInfo.ConsentContent"/>.
    /// </summary>
    [DataContract(Namespace = "")]
    internal sealed class ConsentContent
    {
        /// List of the consent's language versions contained within the <see cref="ConsentInfo.ConsentContent"/>.
        [DataMember]
        public List<ConsentLanguageVersion> ConsentLanguageVersions
        {
            get;
            set;
        }


        /// <summary>
        /// Creates an instance of <see cref="ConsentContent"/> class.
        /// </summary>
        public ConsentContent()
        {
            ConsentLanguageVersions = new List<ConsentLanguageVersion>();
        }
    }
}
