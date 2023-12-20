using CMS.Helpers;

namespace CMS.Protection
{
    /// <summary>
    /// Banned IP control enumeration
    /// Order 1,2,4,8,... is important because of bitwise operation
    /// </summary>
    public enum BanControlEnum
    {
        /// <summary>
        /// Check user IP on Session start, If IP is banned redirect to the information page.
        /// </summary>
        [EnumStringRepresentation("complete")]
        Complete = 1,

        /// <summary>
        /// Check user IP during login.
        /// </summary>
        [EnumStringRepresentation("login")]
        Login = 2,

        /// <summary>
        /// Check user IP during registration.
        /// </summary>
        [EnumStringRepresentation("registration")]
        Registration = 4,

        /// <summary>
        /// (Default) Check user IP during login, registration and input user actions (comments, ratings, ...).
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("allnonecomplete")]
        AllNonComplete = 8
    }
}