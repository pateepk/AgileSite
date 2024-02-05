using System;

using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter events.
    /// </summary>
    public class NewsletterEvents
    {
        /// <summary>
        /// Fired when all newsletter text macros are being resolved. This includes:
        /// - resolving context-free macros for the whole issue before sending
        /// - resolving macros with Subscriber context for each recipient
        /// - resolving macros in the e-mail preview mode
        /// - when link tracking is enabled and tracking link is being changed to the original one, macros in original link are resolved
        /// - ...
        /// </summary>
        public static ResolveMacrosHandler ResolveMacros = new ResolveMacrosHandler { Name = "NewsletterEvents.ResolveMacros" };


        /// <summary>
        /// Fired when issue is being sent in newsletter. If you cancel this event, e-mails won't be generated into newsletter queue (see <see cref="EmailQueueItemInfo"/>) and sent afterwards.
        /// You can use this event to manually add items to queue for your custom subscriber.
        /// </summary>
        /// <remarks>
        /// This event fires either if an issue is being sent to all subscribers (see <see cref="EmailQueueManager.GenerateEmails(IssueInfo)"/>)
        /// or when sent to just one subscriber (<see cref="EmailQueueManager.GenerateEmails(IssueInfo, SubscriberInfo)"/>).
        /// </remarks>
        public static GenerateQueueItemsHandler GenerateQueueItems = new GenerateQueueItemsHandler { Name = "NewsletterEvents.GenerateQueueItems" };
        

        /// <summary>
        /// Event that is called when preview is being generated for a subscriber. Altering of preview content that is being displayed is available through 
        /// <see cref="GeneratePreviewEventArgs.PreviewHtml"/> property.
        /// </summary>
        public static GeneratePreviewHandler GeneratePreview = new GeneratePreviewHandler { Name = "NewsletterEvents.GeneratePreview" };


        /// <summary>
        /// Fired when email of subscriber is being unsubscribed.
        /// </summary>
        public static UnsubscriptionHandler SubscriberUnsubscribes = new UnsubscriptionHandler();


        /// <summary>
        /// Raised when email is opened by the subscriber and tracking of email opening is enabled.
        /// </summary>
        public static LinksHandler SubscriberOpensEmail = new LinksHandler();


        /// <summary>
        /// Raised when subscriber clicks any of the email link and tracking of email link is enabled.
        /// </summary>
        public static LinksHandler SubscriberClicksTrackedLink = new LinksHandler();
    }
}