using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IEmailCampaignEmailGenerator), typeof(EmailCampaignEmailGenerator), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides method for generating emails for given <see cref="IssueInfo"/>.
    /// </summary>
    internal interface IEmailCampaignEmailGenerator
    {
        /// <summary>
        /// Loads all <see cref="ContactInfo"/> subscribed via <see cref="ContactGroupInfo"/> to given <paramref name="issue"/> and fills the given <paramref name="dataTable"/>.
        /// </summary>
        /// <remarks>
        /// It removes email duplicities - e.g. when the <see cref="ContactInfo"/> is subscribed via more <see cref="ContactGroupInfo"/>.
        /// </remarks>
        /// <param name="dataTable">Container to be filled with the generated emails</param>
        /// <param name="issue">Instance of <see cref="IssueInfo"/> the emails are supposed to be generated for</param>
        void GenerateEmailsForIssue(DataTable dataTable, IssueInfo issue);
    }
}