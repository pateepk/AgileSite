namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Interface for a configuration service factory serving initialized <see cref="IZonesConfigurationService"/> service.
    /// </summary>
    /// <exclude />
    public interface IZonesConfigurationServiceFactory
    {
        /// <summary>
        /// Creates initialized <see cref="IZonesConfigurationService"/> service based on given issue identifier.
        /// </summary>
        /// <param name="issueIdentifier">Identifier of the issue.</param>
        /// <returns>Initialized <see cref="IZonesConfigurationService"/> service.</returns>
        IZonesConfigurationService Create(int issueIdentifier);


        /// <summary>
        /// Creates initialized <see cref="IZonesConfigurationService"/> service based on given issue.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <returns>Initialized <see cref="IZonesConfigurationService"/> service.</returns>
        IZonesConfigurationService Create(IssueInfo issue);
    }
}
