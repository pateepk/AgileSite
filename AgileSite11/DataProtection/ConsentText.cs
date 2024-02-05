using CMS.Base;

namespace CMS.DataProtection
{
    /// <summary>
    /// Represents consent's text.
    /// </summary>
    public class ConsentText : AbstractDataContainer<ConsentText>
    {
        /// <summary>
        /// Consent short text.
        /// </summary>
        [RegisterColumn]
        public string ShortText
        {
            get;
            set;
        }


        /// <summary>
        /// Consent full text.
        /// </summary>
        [RegisterColumn]
        public string FullText
        {
            get;
            set;
        }
    }
}
