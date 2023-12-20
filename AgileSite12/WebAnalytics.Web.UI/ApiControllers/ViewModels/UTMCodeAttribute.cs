using System;
using System.ComponentModel.DataAnnotations;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Validates campaign UTM code. UTM code must be unique in every site.
    /// </summary>
    internal class UTMCodeAttribute : ValidationAttribute
    {
        /// <summary>
        /// Checks if object is valid.
        /// </summary>
        /// <param name="value">Object to validate.</param>
        /// <param name="validationContext">Validation context.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            CampaignEditViewModel model = (CampaignEditViewModel)validationContext.ObjectInstance;

            string utmCode = value as string;
            var campaign = CampaignInfoProvider.GetCampaignByUTMCode(utmCode, SiteContext.CurrentSiteName);

            if (!String.IsNullOrEmpty(utmCode) && ((campaign == null) || (campaign.CampaignID == model.CampaignID)))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ResHelper.GetString("campaign.utmcampaign.invalid.duplicate"));
        }
    }
}
