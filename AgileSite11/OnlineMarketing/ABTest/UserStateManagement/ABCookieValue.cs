using System.Collections.Generic;
using System.ComponentModel;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class representing deserialized permanent AB cookie value.
    /// </summary>
    internal class ABCookieValue
    {
        /// <summary>
        /// AB variant name.
        /// </summary>
        [DefaultValue("")]
        public string VariantName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether is visitor included in AB test or not.
        /// If not, Control is always shown and no AB conversions and AB visits are logged.
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
            VariantName = string.Empty;
            Conversions = new List<string>();
        }
    }
}
