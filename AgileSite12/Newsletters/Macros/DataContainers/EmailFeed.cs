using CMS.Base;

namespace CMS.Newsletters
{
    internal class EmailFeed : AbstractDataContainer<EmailFeed>
    {
        protected internal NewsletterInfo Newsletter { get; }


        [RegisterColumn]
        public string Name => Newsletter.NewsletterDisplayName;


        public EmailFeed(NewsletterInfo newsletter)
        {
            Newsletter = newsletter;

            // Set the data container to cache registered columns in the container instance instead of globaly per container type in static field. 
            // This ensures that derived types also have registered their columns.
            UseLocalColumns = true;
        }
    }
}
