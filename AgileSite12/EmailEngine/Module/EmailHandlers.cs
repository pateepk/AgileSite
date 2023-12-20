using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Event handlers for e-mail module
    /// </summary>
    internal class EmailHandlers
    {
        private static readonly string[] SMTP_SERVER_KEYS = { "CMSSMTPServer", "CMSSMTPServerUser", "CMSSMTPServerPassword", "CMSUseSSL" };


        /// <summary>
        /// Initializes the event handlers
        /// </summary>
        public static void Init()
        {
            SettingsKeyInfoProvider.OnSettingsKeyChanged += FlushSmtpServers;
        }


        /// <summary>
        /// Flushes SMTP server lookup table if related settings key changed.
        /// </summary>
        private static void FlushSmtpServers(object sender, SettingsKeyChangedEventArgs e)
        {
            if (SMTP_SERVER_KEYS.Contains(e.KeyName))
            {
                SMTPServerInfoProvider.FlushSMTPServerLookupTable();
            }
        }
    }
}
