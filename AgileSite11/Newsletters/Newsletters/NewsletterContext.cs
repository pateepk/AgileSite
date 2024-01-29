using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter context.
    /// </summary>
    public sealed class NewsletterContext : AbstractContext<NewsletterContext>
    {
        private bool mUnsubscriptionLinksBackwardCompatibilityMode;


        /// <summary>
        /// Indicates that backward compatibility mode is used for newsletter unsubscription links.
        /// In version 8.2 and previous unsubscription links contained contactId parameter. From version 9.0 email parameter is used to identify contact.
        /// </summary>
        public static bool UnsubscriptionLinksBackwardCompatibilityMode
        {
            get
            {
                return Current.mUnsubscriptionLinksBackwardCompatibilityMode;
            }
            set
            {
                Current.mUnsubscriptionLinksBackwardCompatibilityMode = value;
            }
        }
    }
}
