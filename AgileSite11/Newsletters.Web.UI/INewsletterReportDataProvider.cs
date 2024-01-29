using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.Newsletters.Web.UI;

[assembly: RegisterImplementation(typeof(INewsletterReportDataProvider), typeof(NewsletterReportDataProvider), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = Lifestyle.Transient)]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides methods to serve data for newsletter report.
    /// </summary>
    internal interface INewsletterReportDataProvider
    {
        /// <summary>
        /// Returns statistical data of email in the newsletter given by <paramref name="newsletterId"/>.
        /// <para>
        /// Single row of the resulting dataset contains the following data:
        /// <list type="bullet">
        /// <item>
        ///    <description>Email name</description>
        /// </item>
        /// <item>
        ///    <description>Data and time when the email was sent.</description>
        /// </item>
        /// <item>
        ///    <description>Total number of sent emails.</description>
        /// </item>
        /// <item>
        ///    <description>Number of emails opened by their recipients.</description>
        /// </item>
        /// <item>
        ///    <description>Number of links within the emails clicked by their recipients.</description>
        /// </item>
        /// <item>
        ///    <description>Number of recipients who opted-out from the feed.</description>
        /// </item>
        /// <item>
        ///    <description>Relative number of recipients who clicked any link in the email to total number of recipients.</description>
        /// </item>
        /// <item>
        ///    <description>Relative number of recipients who opened the email to total number of recipients.</description>
        /// </item>
        /// <item>
        ///    <description>Relative number of recipients who opted-out to total number of recipients.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="newsletterId">ID of the email feed where the email belong.</param>
        IEnumerable<NewsletterEmailsDataViewModel> GetEmailsData(int newsletterId);
    }
}
