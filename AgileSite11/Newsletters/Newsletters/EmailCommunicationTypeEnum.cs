using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Defines email communication type.
    /// </summary>
    public enum EmailCommunicationTypeEnum
    {
        /// <summary>
        /// Newsletter type.
        /// </summary>
        [EnumStringRepresentation("newsletter")]
        Newsletter = 0,


        /// <summary>
        /// Email campaign type.
        /// </summary>
        [EnumStringRepresentation("campaign")]
        EmailCampaign = 1
    }
}
