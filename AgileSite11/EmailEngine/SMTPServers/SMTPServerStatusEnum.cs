namespace CMS.EmailEngine
{
    /// <summary>
    /// Defines the possible values the SMTP server can exist in while enabled.
    /// </summary>
    internal enum SMTPServerStatusEnum : int
    {
        /// <summary>
        /// The server is not being used at the moment and can be used to send message immediately.
        /// </summary>
        Idle,


        /// <summary>
        /// The server is processing an outstanding message and should not be used to send new messages.
        /// </summary>
        Busy
    }
}