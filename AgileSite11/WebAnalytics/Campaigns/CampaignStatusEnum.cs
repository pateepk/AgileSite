namespace CMS.WebAnalytics
{
    /// <summary>
    /// Represents the current status of a campaign.
    /// </summary>
    public enum CampaignStatusEnum
    {   
        /// <summary>
        /// Campaign have been launched and currently running. All data will be tracked.
        /// </summary>
        Running,


        /// <summary>
        /// Campaign is scheduled to be launched in the future. No data are tracked until launched.
        /// </summary>
        Scheduled,


        /// <summary>
        /// Campaign is not enabled and not ready to be started yet.
        /// </summary>
        Draft,


        /// <summary>
        /// Campaign has finished. No data are tracked.
        /// </summary>
        Finished
    }
}