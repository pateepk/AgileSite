using CMS;
using CMS.Newsletters.Issues.Widgets.Configuration;

[assembly: RegisterImplementation(typeof(IZonesConfigurationServiceFactory), typeof(ZonesConfigurationServiceFactory), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Configuration service factory serving initialized <see cref="IZonesConfigurationService"/> service.
    /// </summary>
    internal sealed class ZonesConfigurationServiceFactory : IZonesConfigurationServiceFactory
    {
        /// <summary>
        /// Creates initialized <see cref="IZonesConfigurationService"/> service based on given issue identifier.
        /// </summary>
        /// <param name="issueIdentifier">Identifier of the issue.</param>
        /// <returns>Initialized <see cref="IZonesConfigurationService"/> service.</returns>
        public IZonesConfigurationService Create(int issueIdentifier)
        {
            var issue = IssueInfoProvider.GetIssueInfo(issueIdentifier);

            return Create(issue);
        }


        /// <summary>
        /// Creates initialized <see cref="IZonesConfigurationService"/> service based on given issue.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <returns>Initialized <see cref="IZonesConfigurationService"/> service.</returns>
        public IZonesConfigurationService Create(IssueInfo issue)
        {
            return new ZonesConfigurationService(issue);
        }
    }
}
