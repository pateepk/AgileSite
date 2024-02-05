using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Constants related to the newsletter module.
    /// </summary>
    public class NewsletterConstants
    {
        /// <summary>
        /// Name of the resolver which is used in second round for subscriber and context macros
        /// </summary>
        internal const string SUBSCRIBERRESOLVERNAME = "subscriber";


        /// <summary>
        /// Name of the resolver which is used in first round for newsletter and issue macros
        /// </summary>
        internal const string NEWSLETTERISSUERESOLVERNAME = "newsletterissue";
    }
}
