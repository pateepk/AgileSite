using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Event arguments for <see cref="GeneratePreviewHandler"/> handler type.
    /// </summary>
    public class GeneratePreviewEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Subscriber for which the e-mail preview is being generated. Can be null if the issue is being generated for generic subscriber.
        /// </summary>
        public SubscriberInfo Subscriber
        {
            get;
            set;
        }


        /// <summary>
        /// Issue for which the e-mail preview is being generated.
        /// </summary>
        public IssueInfo Issue
        {
            get;
            set;
        }


        /// <summary>
        /// Final HTML, that will get shown in preview. Will not modify content if left empty.
        /// </summary>
        public string PreviewHtml
        {
            get;
            set;
        }
    }
}