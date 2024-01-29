namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Specifies when the mapping of Facebook user profile occurs.
    /// </summary>
    public enum FacebookUserProfileMappingTriggerEnum
    {

        /// <summary>
        /// Never.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only when the user signs in.
        /// </summary>
        Registration = 1,

        /// <summary>
        /// Every time the user logs in.
        /// </summary>
        Login = 2

    }
}