using System;
using System.ComponentModel.DataAnnotations;

using CMS.SiteProvider;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Represents View model of Campaign Info which can be serialized and used for communication between API controller and javascript services.
    /// This wrapper has to be used because info objects cannot be easily serialized and deserialized.
    /// </summary>
    public class CampaignEditViewModel
    {
        #region "Properties"

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
        [Required]
        [MaxLength(100)]
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Campaign UTM code. Refers to the <see cref="CampaignInfo.CampaignUTMCode"/>.
        /// </summary>
        [MaxLength(200)]
        [RegularExpression("^[0-9a-zA-Z_.-]+$")]
        [UTMCode]
        public string UTMCode
        {
            get;
            set;
        }


        /// <summary>
        /// Campaign description used in reports, provides closer details about the campaign. Refers to the <see cref="CampaignInfo.CampaignDescription"/>.
        /// </summary>
        public string Description
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
        /// Refers to codename of the campaign. This field is required for enabling single object pinning.
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Refers to site name campaign is assigned to. This field is required for enabling single object pinning.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Empty constructor. Needs to be present to enable data binding performed by the Web API.
        /// </summary>
        public CampaignEditViewModel()
        {
        }


        /// <summary>
        /// Constructor takes <see cref="CampaignInfo"/> and creates new instance of <see cref="CampaignEditViewModel"/> containing all values
        /// required for communication between Web API and javascript modules.
        /// </summary>
        /// <param name="campaign">Campaign info obtained from the database</param>
        /// <param name="dateTime">Datetime, used to calculate the current status of campaign</param>
        /// <exception cref="ArgumentNullException"><paramref name="campaign"/> is null</exception>
        public CampaignEditViewModel(CampaignInfo campaign, DateTime dateTime)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }

            CampaignID = campaign.CampaignID;
            DisplayName = campaign.CampaignDisplayName;
            UTMCode = campaign.CampaignUTMCode;
            Description = campaign.CampaignDescription;
            Status = campaign.GetCampaignStatus(dateTime).ToString();
            SiteName = SiteInfoProvider.GetSiteName(campaign.CampaignSiteID);
            CodeName = campaign.CampaignName;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Fills given <paramref name="campaign"/> with data obtained from current instance of <see cref="CampaignEditViewModel"/>. 
        /// This method should be used whenever the call from client to the server with filled <see cref="CampaignEditViewModel"/> was made
        /// and the incoming object values need to be copied to the existing <paramref name="campaign"/>.
        /// </summary>
        /// <param name="campaign">Campaign which will be filled with data from the current instance of <see cref="CampaignEditViewModel"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="campaign"/> is null</exception>
        /// <returns><paramref name="campaign"/> filled with the data from current instance of <see cref="CampaignEditViewModel"/></returns>
        public CampaignInfo FillCampaignInfo(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }

            campaign.CampaignDisplayName = DisplayName;
            campaign.CampaignDescription = Description;
            campaign.CampaignUTMCode = UTMCode;
            return campaign;
        }

        #endregion
    }
}