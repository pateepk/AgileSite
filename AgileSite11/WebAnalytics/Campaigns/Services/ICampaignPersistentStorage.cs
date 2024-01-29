namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to storing and retrieving campaign code, campaign source name and campaign content from/to persistent storage.
    /// Persistent storage is a place where campaign code and source name can be stored and after the same visitor
    /// makes another request, it will be returned.
    /// </summary>
    public interface ICampaignPersistentStorage
    {
        /// <summary>
        /// Campaign UTM code.
        /// </summary>
        string CampaignUTMCode
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the campaign's source name.
        /// </summary>
        string SourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Campaign UTM content.
        /// </summary>
        string CampaignUTMContent
        {
            get;
            set;
        }
    }
}