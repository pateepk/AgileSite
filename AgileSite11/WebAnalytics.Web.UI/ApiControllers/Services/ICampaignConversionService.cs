namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Service that provides method to work with the <see cref="CampaignConversionViewModel"/> objects.
    /// </summary>
    public interface ICampaignConversionService
    {
        /// <summary>
        /// Returns view model from given campaign conversion info.
        /// </summary>
        /// <param name="info">Campaign conversion info</param>
        CampaignConversionViewModel GetConversionViewModel(CampaignConversionInfo info);


        /// <summary>
        /// Sets data from campaign conversion model and returns updated view model.
        /// </summary>
        /// <param name="model">Campaign conversion view model</param>
        CampaignConversionViewModel SaveConversion(CampaignConversionViewModel model);
    }
}
