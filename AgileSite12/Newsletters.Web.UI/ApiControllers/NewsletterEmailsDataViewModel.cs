using System;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Represents data container to be used for newsletter report.
    /// </summary>
    public class NewsletterEmailsDataViewModel
    {
        /// <summary>
        /// Display name of the email.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Date when the email was sent.
        /// </summary>
        public DateTime Date { get; set; }


        /// <summary>
        /// Absolute number of recipients whom the email was sent to.
        /// </summary>
        public int Sent { get; set; }


        /// <summary>
        /// Absolute number of recipients who opened the email.
        /// </summary>
        public int Opens { get; set; }


        /// <summary>
        /// Absolute number of recipients who clicked any link in the email.
        /// </summary>
        public int Clicks { get; set; }


        /// <summary>
        /// Absolute number of recipients who opted-out.  
        /// </summary>
        public int Unsubscribed { get; set; }


        /// <summary>
        /// Relative number of recipients who clicked any link in the email to total number of recipients.
        /// </summary>
        public decimal ClickRate { get; set; }


        /// <summary>
        /// Relative number of recipients who opened the email to total number of recipients.
        /// </summary>
        public decimal OpenRate { get; set; }


        /// <summary>
        /// Relative number of recipients who opted-out to total number of recipients.
        /// </summary>
        public decimal UnsubscribedRate { get; set; }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public NewsletterEmailsDataViewModel()
        {
        }
    }
}