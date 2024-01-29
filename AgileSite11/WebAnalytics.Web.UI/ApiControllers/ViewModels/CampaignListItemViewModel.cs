using System;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Represents listing View model of Campaign Info which can be serialized and used for communication between API controller and javascript services.
    /// This wrapper has to be used because info objects cannot be easily serialized and deserialized.
    /// </summary>
    public class CampaignListItemViewModel
    {
        /// <summary>
        /// Unique integer identifier of the campaign. Refers to the <see cref="CampaignInfo.CampaignID"/>.
        /// </summary>
        public int CampaignID
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the campaign used in listing and all reports. Refers to the <see cref="CampaignInfo.CampaignDisplayName"/>.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Number of days from now to campaign start.
        /// </summary>
        public int DaysToStart
        {
            get;
            set;
        }


        /// <summary>
        /// Provides time from which the campaign will be running. Refers to the <see cref="CampaignInfo.CampaignOpenFrom"/>.
        /// </summary>
        public DateTime? OpenFrom
        {
            get;
            set;
        }


        /// <summary>
        /// Provides time to which the champaign will be running. Refers to the <see cref="CampaignInfo.CampaignOpenTo"/>.
        /// </summary>
        public DateTime? OpenTo
        {
            get;
            set;
        }


        /// <summary>
        /// Refers to the current status. Can be one of the <see cref="CampaignStatusEnum"/>.
        /// </summary>
        public string Status
        {
            get;
            set;
        }


        /// <summary>
        /// Single object url for editing current campaign.
        /// </summary>
        public string DetailLink
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates how many conversion happened in campaign.
        /// </summary>
        public int Conversions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates how many visitor has reached campaign.
        /// </summary>
        public int Visitors
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates how the campaign objective stored in <see cref="CampaignObjectiveInfo"/> is met (in percents).
        /// If no objective is set for the campaign, this property is <c>null</c>.
        /// </summary>
        public decimal? Objective
        {
            get;
            set;
        }
    }
}
