using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.WebApi;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterCMSApiController(typeof(AssetController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Handles the campaign assets.
    /// </summary>
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.WEBANALYTICS, "ManageCampaigns")]
    [HandleExceptions]
    public sealed class AssetController : CMSApiController
    {
        /// <summary>
        /// Performs saving of the <see cref="CampaignAssetInfo"/> corresponding to the given <see cref="CampaignAssetViewModel"/>.
        /// The <see cref="CampaignAssetInfo"/> is inserted with the values obtained from <paramref name="campaignAssetViewModel"/>.
        /// </summary>
        /// <param name="campaignAssetViewModel">Instance of updated view-model. This instance will be used to creating corresponding <see cref="CampaignAssetInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad request</c>, if model state of binded <paramref name="campaignAssetViewModel"/> was not valid;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <paramref name="campaignAssetViewModel"/>. This campaign has <see cref="CampaignEditViewModel.CampaignID"/> filled
        /// with the data from the DB.
        /// </returns>
        [HttpPost]
        public CampaignAssetViewModel Post([FromBody] CampaignAssetViewModel campaignAssetViewModel)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            CheckAssetCanBeModified(campaignAssetViewModel.CampaignID);

            var service = Service.Resolve<ICampaignAssetModelService>().GetStrategy(campaignAssetViewModel.Type);
            var info = service.GetAssetInfo(campaignAssetViewModel);
            
            // Check if this asset already exists within campaign
            var existingAsset = GetExistingAsset(info);
            if (existingAsset != null)
            {
                return service.GetAssetViewModel(existingAsset);
            }

            return service.SetAssetInfo(campaignAssetViewModel);
        }


        /// <summary>
        /// Performs update of the <see cref="CampaignAssetInfo"/> corresponding.
        /// The <see cref="CampaignAssetInfo" /> values are updated with the values from <paramref name="campaignAssetViewModel"/>.
        /// </summary>
        /// <remarks>
        /// No dirty checking is performed, <see cref="CampaignAssetInfo"/> will be updated even if no change was made to the object.
        /// HTTP verb PUT is not supported, therefore this request accepts only the POST requests.
        /// </remarks>
        /// <param name="campaignAssetViewModel">Instance of updated view-model. This instance will be used to update corresponding <see cref="CampaignAssetInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad request</c>, if model state of binded <paramref name="campaignAssetViewModel"/> was not valid
        /// otherwise, <c>HTTP status code 200 OK</c> with updated <paramref name="campaignAssetViewModel"/>.
        /// </returns>
        [HttpPost]
        public CampaignAssetViewModel Put([FromBody] CampaignAssetViewModel campaignAssetViewModel)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            CheckAssetCanBeModified(campaignAssetViewModel.CampaignID);
            CheckUTMSourceCanBeModified(campaignAssetViewModel);

            return Service.Resolve<ICampaignAssetModelService>().GetStrategy(campaignAssetViewModel.Type).SetAssetInfo(campaignAssetViewModel);
        }        


        /// <summary>
        /// Deletes given campaign asset.
        /// </summary>
        /// <remarks>
        /// When response is returned, there will be no campaign with given ID in database.
        /// HTTP verb DELETE is not supported, therefore this request accepts only the GET requests.
        /// </remarks>
        /// <param name="campaignAssetID">ID of asset</param>
        /// <returns>
        /// Returns <c>HTTP status code 200 OK</c> if request was successful.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Delete(int campaignAssetID)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            
            var campaignAsset = CampaignAssetInfoProvider.GetCampaignAssetInfo(campaignAssetID);
            if (campaignAsset != null)
            {
                CheckAssetCanBeModified(campaignAsset.CampaignAssetCampaignID);
                CampaignAssetInfoProvider.DeleteCampaignAssetInfo(campaignAsset);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        private static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(message)
            };
        }


        private void CheckAssetCanBeModified(int campaignId)
        {
            // Assets can be modified within a draft campaign only
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignId);
            if ((campaign != null) && (campaign.GetCampaignStatus(DateTime.Now) == CampaignStatusEnum.Finished))
            {
                var message = ResHelper.GetString("campaign.asset.cannotmodify");
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }
        }


        private static void CheckUTMSourceCanBeModified(CampaignAssetViewModel campaignAssetViewModel)
        {
            object isUTMSourceEditable;
            campaignAssetViewModel.AdditionalProperties.TryGetValue("isEditable", out isUTMSourceEditable);
            if (!isUTMSourceEditable.ToBoolean(true))
            {
                var message = ResHelper.GetString("campaign.asset.utmsource.cannotmodify");
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }
        }


        private CampaignAssetInfo GetExistingAsset(CampaignAssetInfo info)
        {
            return CampaignAssetInfoProvider.GetCampaignAssets()
                                         .WhereEquals("CampaignAssetType", info.CampaignAssetType)
                                         .WhereEquals("CampaignAssetAssetGuid", info.CampaignAssetAssetGuid)
                                         .WhereEquals("CampaignAssetCampaignID", info.CampaignAssetCampaignID)
                                         .FirstObject;
        }
    }
}
