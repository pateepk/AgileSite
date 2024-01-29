using CMS.Base;

namespace CMS.Newsletters
{
    internal class Email : AbstractDataContainer<Email>
    {
        protected readonly NewsletterInfo newsletter;
        protected readonly IssueInfo issue;


        #region Properties

        [RegisterColumn]
        public string Name => issue.IssueDisplayName;


        [RegisterColumn]
        public string Subject => issue.IssueSubject;


        [RegisterColumn]
        public string SenderName => issue.IssueSenderName;


        [RegisterColumn]
        public string SenderEmail => issue.IssueSenderEmail;


        [RegisterColumn]
        public string Preheader => issue.IssuePreheader;


        #endregion


        public Email(NewsletterInfo newsletter, IssueInfo issue)
        {
            this.newsletter = newsletter;
            this.issue = issue;

            // Set the data container to cache registered columns in the container instance instead of globaly per container type in static field. 
            // This ensures that derived types also have registered their columns.
            UseLocalColumns = true;
        }
    }
}
