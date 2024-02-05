namespace CMS.Synchronization
{
    /// <summary>
    /// Server authentication modes.
    /// </summary>
    public enum ServerAuthenticationEnum
    {
        /// <summary>
        /// User name / password authentication.
        /// </summary>
        UserName = 1,

        /// <summary>
        /// X509 authentication.
        /// </summary>
        X509 = 2
    }
}