namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the SQL authentication mode.
    /// </summary>
    public enum SQLServerAuthenticationModeEnum : int
    {
        /// <summary>
        /// SQL Server authentication.
        /// </summary>
        SQLServerAuthentication = 0,

        /// <summary>
        /// Windows authentication.
        /// </summary>
        WindowsAuthentication = 1,
    }
}