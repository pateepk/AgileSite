using CMS.Helpers;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Specifies visibility of a LinkedIn share.
    /// </summary>
    /// <remarks>The enum is currently internal because <see cref="Anyone"/> is the only working option.</remarks>
    internal enum LinkedInVisibilityCodeEnum
    {
        /// <summary>
        /// All members.
        /// </summary>
        [EnumStringRepresentation("anyone")]
        Anyone = 1,


        /// <summary>
        /// Connections only.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Despite the fact the documentation for company shares claims 'connections-only' is a valid option, the API response disagrees.
        /// 400: Invalid visibility for company share: {connections-only}, only 'anyone' or 'dark' is allowed.
        /// </para>
        /// <para>
        /// Publishing with 'dark' visibility results in
        /// 403: Dark post is not allowed for this app.
        /// </para>
        /// </remarks>
        [EnumStringRepresentation("connections-only")]
        ConnectionsOnly = 2
    }
}
