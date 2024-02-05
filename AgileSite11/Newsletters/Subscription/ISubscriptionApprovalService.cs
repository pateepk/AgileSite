using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Approves subscription by provided hash.
    /// </summary>
    public interface ISubscriptionApprovalService
    {
        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true and SubscriptionApprovedWhen to current time. 
        /// Checks if subscription wasn't already approved. Confirmation e-mail may be sent optionally.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, peforms logging of the subscription logging activity.
        /// </remarks>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation e-mail should be sent. Confirmation e-mail may also be sent if newsletter settings requires so</param>
        /// <param name="siteName">Site name</param>
        /// <param name="datetime">Date and time of request.</param>
        ApprovalResult ApproveSubscription(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime);
    }
}
