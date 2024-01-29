using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Specifies possible values of Marketing email setting displayed within the contact card.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactMarketingEmailStatusEnum
    {
        /// <summary>
        /// Receiving marketing emails.
        /// </summary>
        ReceivingMarketingEmails,

        /// <summary>
        /// The system does not have an email address for this contact. The contact is not receiving marketing emails and can't be added to any recipient list.
        /// </summary>
        NoEmailAddress,

        /// <summary>
        /// Determines the contact is unsubscribed from all the marketing communication.
        /// </summary>
        OptedOut,

        /// <summary>
        /// The contact reached the maximal count of bounces and does not receive marketing emails any more.
        /// </summary>
        Undeliverable
    }
}