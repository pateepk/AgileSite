using System.Collections.Generic;

namespace CMS.Newsletters.Issues.Widgets.Content
{
    /// <summary>
    /// Represents zone within an email content.
    /// </summary>
    public sealed class EmailZone
    {
        /// <summary>
        /// Zone identifier.
        /// </summary>
        public string Identifier
        {
            get;
        }


        /// <summary>
        /// Widgets within the zone.
        /// </summary>
        public ICollection<WidgetContent> Widgets
        {
            get;
        }


        /// <summary>
        /// Creates an instance of <see cref="EmailZone"/> class.
        /// </summary>
        /// <param name="identifier">Zone identifier.</param>
        public EmailZone(string identifier)
        {
            Identifier = identifier;
            Widgets = new List<WidgetContent>();
        }
    }
}
