using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Core;
using CMS.Helpers;
using CMS.WebAnalytics.Web.UI;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(CampaignConversionController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Handles the campaign conversions.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.WEBANALYTICS, "ManageCampaigns")]
    [HandleExceptions]
    public sealed class CampaignConversionController : CMSApiController
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CampaignConversionInfo"/> corresponding to the given <see cref="CampaignConversionViewModel"/>.
        /// The <see cref="CampaignConversionInfo"/> is inserted into database with the values obtained from <paramref name="conversionViewModel"/>.
        /// </summary>
        /// <remarks>
        /// If conversion with the same name and activity already exists within campaign, no action is performed and exiting conversion is returned.
        /// </remarks>
        /// <param name="conversionViewModel">Instance of view model with conversion information. This instance will be used to create corresponding <see cref="CampaignConversionInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if model state of bound <paramref name="conversionViewModel"/> is not valid;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <paramref name="conversionViewModel"/>.
        /// </returns>
        [HttpPost]
        public CampaignConversionViewModel Post([FromBody] CampaignConversionViewModel conversionViewModel)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            CheckCanModifyConversion(conversionViewModel.CampaignID);

            // Check if this conversion already exists within campaign
            var existingConversion = GetExistingConversion(conversionViewModel);
            if (existingConversion != null)
            {
                return Service.Resolve<ICampaignConversionService>().GetConversionViewModel(existingConversion);
            }

            return SetConversion(conversionViewModel);
        }


        /// <summary>
        /// Updates existing instance of the <see cref="CampaignConversionInfo"/> corresponding to the given <see cref="CampaignConversionViewModel"/>.
        /// The <see cref="CampaignConversionInfo"/> is updated in database with the values obtained from <paramref name="conversionViewModel"/>.
        /// </summary>
        /// <remarks>
        /// If conversion with the same name and activity already exists within campaign, no action is performed and original unchanged conversion is returned.
        /// </remarks>
        /// <param name="conversionViewModel">Instance of view model with conversion information. This instance will be used to update corresponding <see cref="CampaignConversionInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if model state of bound <paramref name="conversionViewModel"/> is not valid;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <paramref name="conversionViewModel"/>.
        /// </returns>
        [HttpPost]
        public CampaignConversionViewModel Put([FromBody] CampaignConversionViewModel conversionViewModel)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            CheckCanModifyConversion(conversionViewModel.CampaignID);

            // Check if this conversion already exists within campaign
            var existingConversion = GetExistingConversion(conversionViewModel);
            if (existingConversion != null)
            {
                var originalConversion = CampaignConversionInfoProvider.GetCampaignConversionInfo(conversionViewModel.ID);
                return Service.Resolve<ICampaignConversionService>().GetConversionViewModel(originalConversion);
            }

            return SetConversion(conversionViewModel);
        }


        /// <summary>
        /// Deletes the given campaign conversion.
        /// </summary>
        /// <remarks>
        /// When response is returned, there will be no conversion with given ID in the database.
        /// HTTP verb DELETE is not supported, therefore this request accepts only GET requests.
        /// </remarks>
        /// <param name="conversionID">ID of conversion</param>
        /// <returns>
        /// Returns <c>HTTP status code 200 OK</c> if request is successful.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Delete(int conversionID)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var conversion = CampaignConversionInfoProvider.GetCampaignConversionInfo(conversionID);
            if (conversion != null)
            {
                var campaign = CampaignInfoProvider.GetCampaignInfo(conversion.CampaignConversionCampaignID);
                var status = campaign.GetCampaignStatus(DateTime.Now);

                if (status == CampaignStatusEnum.Finished)
                {
                    var message = ResHelper.GetString("campaign.conversion.removefinished");
                    throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
                }

                var canBeConversionDeleted = Service.Resolve<ICampaignValidationService>().CanBeConversionDeleted(campaign);
                if ((status != CampaignStatusEnum.Draft) && !canBeConversionDeleted && !conversion.CampaignConversionIsFunnelStep)
                {
                    var message = ResHelper.GetString("campaign.conversion.atleastone");
                    throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
                }

                CampaignConversionInfoProvider.DeleteCampaignConversionInfo(conversion);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        private CampaignConversionInfo GetExistingConversion(CampaignConversionViewModel conversion)
        {
            return CampaignConversionInfoProvider.GetCampaignConversions()
                .WhereEquals("CampaignConversionCampaignID", conversion.CampaignID)
                .WhereEquals("CampaignConversionActivityType", conversion.ActivityType)
                .WhereEquals("CampaignConversionItemID", conversion.ItemID)
                .WhereEquals("CampaignConversionIsFunnelStep", conversion.IsFunnelStep)
                .WhereEquals("CampaignConversionURL", conversion.Url)
                .WhereNotEquals("CampaignConversionID", conversion.ID)
                .TopN(1)
                .FirstOrDefault();
        }


        private void CheckCanModifyConversion(int campaignId)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignId);
            if (campaign.GetCampaignStatus(DateTime.Now) == CampaignStatusEnum.Finished)
            {
                var message = ResHelper.GetString("campaign.conversion.modifyconversion");
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }
        }


        private CampaignConversionViewModel SetConversion(CampaignConversionViewModel conversion)
        {
            var updated = Service.Resolve<ICampaignConversionService>().SaveConversion(conversion);

            // Invalidate campaign statistics
            Service.Resolve<ICampaignReportService>().InvalidateCampaignReport(conversion.CampaignID);

            return updated;
        }


        private static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(message)
            };
        }
    }
}
