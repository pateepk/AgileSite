using System.Runtime.Serialization;

namespace CMS.DataProtection
{
    /// <summary>
    /// Represents language version of consent contained within the <see cref="ConsentInfo.ConsentContent"/>.
    /// </summary>
    [DataContract(Namespace = "")]
    internal sealed class ConsentLanguageVersion
    {
        /// <summary>
        /// Culture code of consent language version.
        /// </summary>
        [DataMember]
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Short text of consent language version.
        /// </summary>
        [DataMember]
        public string ShortText
        {
            get;
            set;
        }


        /// <summary>
        /// Full text of consent language version.
        /// </summary>
        [DataMember]
        public string FullText
        {
            get;
            set;
        }
    }
}
