using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Email template type enumeration.
    /// </summary>
    public enum EmailTemplateTypeEnum
    {
        /// <summary>
        /// Newsletter issue template.
        /// </summary>
        [EnumStringRepresentation("I")]
        Issue,


        /// <summary>
        /// Newsletter subscription template.
        /// </summary>
        [EnumStringRepresentation("S")]
        Subscription,


        /// <summary>
        /// Newsletter unsubscription template.
        /// </summary>
        [EnumStringRepresentation("U")]
        Unsubscription,


        /// <summary>
        /// Double opt-in activation template.
        /// </summary>
        [EnumStringRepresentation("D")]
        DoubleOptIn
    }
}
