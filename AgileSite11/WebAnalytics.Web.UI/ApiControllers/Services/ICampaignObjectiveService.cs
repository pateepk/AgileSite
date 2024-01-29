
namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignObjectiveViewModel"/> objects.
    /// </summary>
    public interface ICampaignObjectiveService
    {
        /// <summary>
        /// Returns view model from specified objective (<paramref name="info"/>).
        /// </summary>
        /// <param name="info">Campaign objective info</param>
        CampaignObjectiveViewModel GetObjectiveViewModel(CampaignObjectiveInfo info);


        /// <summary>
        /// Sets data from campaign objective model <paramref name="model"/> and returns updated view model.
        /// </summary>
        /// <param name="model">Campaign objective view model</param>
        CampaignObjectiveViewModel SaveObjective(CampaignObjectiveViewModel model);
    }
}
