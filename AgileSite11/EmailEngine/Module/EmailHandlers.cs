using CMS.Base;
using CMS.DataEngine;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Event handlers for e-mail module 
    /// </summary>
    internal class EmailHandlers
    {
        /// <summary>
        /// Initializes the event handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += CancelEmailSending;
        }


        /// <summary>
        /// Cancels sending of emails if related settings key changed
        /// </summary>
        private static void CancelEmailSending(object sender, SettingsKeyChangedEventArgs e)
        {
            if (e.KeyName.EqualsCSafe("CMSEmailsEnabled", true) && (e.SiteID <= 0) && !e.KeyValue.ToBoolean(false))
            {
                // Stop current sending of e-mails and newsletters if e-mails are disabled in global settings
                EmailHelper.Queue.CancelSending();
            }
        }
    }
}
