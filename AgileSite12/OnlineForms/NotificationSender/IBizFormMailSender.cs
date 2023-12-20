namespace CMS.OnlineForms
{
    /// <summary>
    /// Interface providing methods for sending on-line form notification and autoresponder emails.
    /// </summary>
    public interface IBizFormMailSender
    {
        /// <summary>
        /// Sends notification email to specified person based on on-line form configuration and collected data.
        /// </summary>
        void SendNotificationEmail();


        /// <summary>
        /// Sends confirmation email (autoresponder) based on on-line form configuration and collected data.
        /// </summary>
        void SendConfirmationEmail();
    }
}
