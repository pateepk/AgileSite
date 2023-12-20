using CMS.Helpers;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Specifies visibility of a LinkedIn share.
    /// </summary>
    internal enum LinkedInMemberNetworkVisibilityEnum
    {
        /// <summary>
        /// The share will be viewable by anyone on LinkedIn.
        /// </summary>
        [EnumStringRepresentation("PUBLIC")]
        Public = 1,


        /// <summary>
        /// The share will be viewable by 1st-degree connections only.
        /// </summary>
        [EnumStringRepresentation("CONNECTIONS")]
        ConnectionsOnly = 2,


        /// <summary>
        /// Viewable by logged in members only.
        /// </summary>
        [EnumStringRepresentation("LOGGED_IN")]
        LoggedInOnly = 3
    }
}
