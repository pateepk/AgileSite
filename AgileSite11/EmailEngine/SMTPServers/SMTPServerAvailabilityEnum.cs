namespace CMS.EmailEngine
{
    /// <summary>
    /// Defines the possible values of SMTP server availability that occur when trying to obtain a new connection.
    /// </summary>
    internal enum SMTPServerAvailabilityEnum : int
    {
        /// <summary>
        /// This value indicates that a certain SMTP server is available to service the request.
        /// Callers can proceed with their request.
        /// </summary>
        Available,


        /// <summary>
        /// This value indicates that no SMTP server is available at the moment to service the request.
        /// Callers should wait and retry later to obtain available SMTP server.
        /// </summary>
        TemporarilyUnavailable,


        /// <summary>
        /// This value indicates that no SMTP servers are available to service the request.
        /// Callers should not wait and can quit immediately.
        /// </summary>
        PermanentlyUnavailable
    }
}