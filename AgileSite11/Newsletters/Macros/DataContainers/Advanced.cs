using CMS.Base;

namespace CMS.Newsletters
{
    internal class Advanced : AbstractDataContainer<Advanced>
    {
        #region Properties

        [RegisterColumn]
        public NewsletterInfo NewsletterInfo
        {
            get;
            set;
        }


        [RegisterColumn]
        public IssueInfo IssueInfo
        {
            get;
            set;
        }

        #endregion


        public Advanced(NewsletterInfo newsletter, IssueInfo issue)
        {
            NewsletterInfo = newsletter;
            IssueInfo = issue;

            // Set the data container to cache registered columns in the container instance instead of globaly per container type in static field. 
            // This ensures that derived types also have registered their columns.
            UseLocalColumns = true;
        }
    }
}
