using System.Collections.Generic;
using System.ComponentModel;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Class representing deserialized permanent A/B cookie value.
    /// </summary>
    internal class ABCookieValue<TIdentifier>
    {
        /// <summary>
        /// A/B variant identifier.
        /// </summary>
        public TIdentifier VariantIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether is visitor included in A/B test or not.
        /// If not, Control is always shown and no A/B conversions and A/B visits are logged.
        /// </summary>
        [DefaultValue(false)]
        public bool ExcludedFromTest
        {
            get;
            set;
        }


        /// <summary>
        /// Conversions performed during this test.
        /// </summary>
        public IList<string> Conversions
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor. Sets properties to empty values.
        /// </summary>
        public ABCookieValue()
        {
            Conversions = new List<string>();
        }
    }
}
