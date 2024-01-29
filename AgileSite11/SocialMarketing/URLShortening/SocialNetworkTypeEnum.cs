using System;
using System.Linq;
using System.Text;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Types of social networks.
    /// </summary>
    public enum SocialNetworkTypeEnum
    {
        /// <summary>
        /// Twitter
        /// </summary>
        Twitter,

        /// <summary>
        /// Facebook
        /// </summary>
        Facebook,

        /// <summary>
        /// LinkedIn
        /// </summary>
        LinkedIn,

        /// <summary>
        /// Google+
        /// </summary>
        GooglePlus,

        /// <summary>
        /// None - Error state - null replacement
        /// </summary>
        None,
    }
}
