using System;

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for service retrieving URLs for issue content.
    /// </summary>
    public interface IIssueUrlService
    {
        /// <summary>
        /// Gets the base URL for given <paramref name="newsletter"/> if defined. Otherwise it returns full application URL of newsletter site. 
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newsletter"/> is <c>null</c></exception>
        /// <remarks>The result URL doesn't contain trailing slash.</remarks>
        string GetBaseUrl(NewsletterInfo newsletter);


        /// <summary>
        /// Gets unsubscription base URL for given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        string GetUnsubscriptionBaseUrl(NewsletterInfo newsletter);


        /// <summary>
        /// Gets unsubscription URL for given <paramref name="issue"/>, <paramref name="newsletter"/>, and <paramref name="subscriber"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>
        string GetUnsubscriptionUrl(NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber);


        /// <summary>
        /// Gets activation base URL for the newsletter opt-in feature.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        string GetActivationBaseUrl(NewsletterInfo newsletter);


        /// <summary>
        /// Creates activation URL for the newsletter opt-in feature.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="subscriber">Subscriber.</param>
        /// <param name="subscription">Subscription.</param>
        /// <returns>Activation URL for given <paramref name="newsletter"/> and <paramref name="subscriber"/>.</returns>
        /// <remarks>When the activation URL is created, the hash used for the activation URL is stored to the related <paramref name="subscription"/>.</remarks>
        string CreateActivationUrl(NewsletterInfo newsletter, SubscriberInfo subscriber, SubscriberNewsletterInfo subscription);


        /// <summary>
        /// Gets base URL to view email content in a browser.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        string GetViewInBrowserBaseUrl(NewsletterInfo newsletter);


        /// <summary>
        /// Gets URL to view email content in a browser.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>
        string GetViewInBrowserUrl(NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber);


        /// <summary>
        /// Gets resolved source URL of dynamic newsletter.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        string GetDynamicNewsletterUrl(NewsletterInfo newsletter);
    }
}