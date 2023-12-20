using System;

using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handler for event <see cref="NewsletterEvents.GeneratePreview"/>. Alteration of what is being displayed as a preview e-mail for a subscriber can
    /// be done through 
    /// </summary>
    public class GeneratePreviewHandler : AdvancedHandler<GeneratePreviewHandler, GeneratePreviewEventArgs>
    {
        /// <summary>
        /// Starts the event
        /// </summary>
        /// <param name="issue">Issue for which the e-mail preview is being generated</param>
        /// <param name="subscriber">Subscriber for which the e-mail preview is being generated</param>
        /// <param name="previewHtml">Final HTML, that will get shown in preview. This parameter is readable from <see cref="GeneratePreviewEventArgs.PreviewHtml"/> property when the event is finished.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        public GeneratePreviewHandler Start(IssueInfo issue, SubscriberInfo subscriber, string previewHtml = null)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            var args = new GeneratePreviewEventArgs
            {
                Subscriber = subscriber,
                Issue = issue,
                PreviewHtml = previewHtml
            };

            return StartEvent(args);
        }
    }
}