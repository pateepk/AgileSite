using System.Collections.Generic;

namespace CMS.Newsletters.Issues.Widgets.Content
{
    /// <summary>
    /// Represents the content of an email including zones with widgets' content.
    /// </summary>
    public sealed class EmailContent
    {
        /// <summary>
        /// Zones with widget content.
        /// </summary>
        public ICollection<EmailZone> Zones
        {
            get;
        }


        /// <summary>
        /// Creates an instance of <see cref="EmailContent"/> class.
        /// </summary>
        public EmailContent()
        {
            Zones = new List<EmailZone>();
        }
    }
}
