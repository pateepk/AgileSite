using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Web farm task used to notify that one of the settings affecting whether email sending is allowed has changed.
    /// </summary>
    internal class EmailsEnabledSettingChangedWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Notifies that one of the settings affecting whether email sending is allowed has changed.
        /// </summary>
        public override void ExecuteTask()
        {
            ThreadEmailSender.NotifyEmailsEnabledSettingChanged(false);
        }
    }
}
