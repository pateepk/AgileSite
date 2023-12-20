using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics.Web.UI;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(CampaignController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Handles getting of campaigns.
    /// </summary>
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.WEBANALYTICS, "ManageCampaigns")]
    [HandleExceptions]
    public sealed class CampaignController : CMSApiController
    {
        /// <summary>
        /// Gets list of campaigns from the current site.
        /// </summary>
        /// <returns>
        /// HTTP response message containing the status code dependent on whether the action was successful or not.
        /// Returns 200 OK status and list of campaigns from the current site.
        /// </returns>
        public IEnumerable<CampaignEditViewModel> Get()
        {
            var now = DateTime.Now;
            return CampaignInfoProvider.GetCampaigns().OnSite(SiteContext.CurrentSiteID).Select(campaign => new CampaignEditViewModel(campaign, now));
        }


        /// <summary>
        /// Performs saving of the <see cref="CampaignInfo"/> corresponding to the given <see cref="CampaignEditViewModel"/>.
        /// The <see cref="CampaignInfo"/> is inserted with the values obtained from <paramref name="model"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CampaignInfo.CampaignName"/> is set to <see cref="InfoHelper.CODENAME_AUTOMATIC"/> to let the <see cref="CampaignInfoProvider"/> decide
        /// what codename should be used based on the <see cref="CampaignInfo.CampaignDisplayName"/>.
        /// </remarks>
        /// <param name="model">Instance of updated view-model. This instance will be used to create corresponding <see cref="CampaignInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad request</c>, if model state of bound <paramref name="model"/> was not valid;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <paramref name="model"/>. This campaign has <see cref="CampaignEditViewModel.CampaignID"/> filled
        /// with the data from the DB.
        /// </returns>
        public CampaignEditViewModel Post([FromBody] CampaignEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var campaignInfo = new CampaignInfo();
            model.FillCampaignInfo(campaignInfo);
            campaignInfo.CampaignName = "Campaign_" + Guid.NewGuid();
            campaignInfo.CampaignSiteID = SiteContext.CurrentSiteID;

            CampaignInfoProvider.SetCampaignInfo(campaignInfo);
            model.CampaignID = campaignInfo.CampaignID;
            model.CodeName = campaignInfo.CampaignName;

            return model;
        }


        /// <summary>
        /// Performs update of the <see cref="CampaignInfo"/> corresponding to the given <see cref="CampaignEditViewModel.CampaignID"/> property.
        /// The <see cref="CampaignInfo" /> values are updated with the values from <paramref name="model"/>. 
        /// </summary>
        /// <remarks>
        /// No dirty checking is performed, <see cref="CampaignInfo"/> will be updated even if no change was made to the object.
        /// HTTP verb PUT is not supported, therefore this request accepts only the POST requests.
        /// </remarks>
        /// <param name="model">Instance of updated view-model. This instance will be used to update corresponding <see cref="CampaignInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad request</c>, if model state of bound <paramref name="model"/> was not valid or -
        /// if no campaign was found for the provided <see cref="CampaignEditViewModel.CampaignID"/>;
        /// otherwise, <c>HTTP status code 200 OK</c> with updated <paramref name="model"/>. 
        /// </returns>
        [HttpPost]
        public CampaignEditViewModel Put([FromBody] CampaignEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var campaignInfo = CampaignInfoProvider.GetCampaignInfo(model.CampaignID);
            CheckCampaignExists(campaignInfo);

            if (campaignInfo.CampaignUTMCode != model.UTMCode)
            {
                UpdateNewsletterUTMCodes(campaignInfo.CampaignUTMCode, model.UTMCode);
            }

            model.FillCampaignInfo(campaignInfo);
            CampaignInfoProvider.SetCampaignInfo(campaignInfo);

            model.CodeName = campaignInfo.CampaignName;

            return model;
        }


        /// <summary>
        /// Deletes campaign with given id only for current site.
        /// </summary>
        /// <remarks>
        /// When response is returned, there will be no campaign with given ID in database.
        /// HTTP verb DELETE is not supported, therefore this request accepts only the GET requests.
        /// </remarks>
        /// <param name="id">ID of campaign</param>
        /// <returns>
        /// Returns <c>HTTP status code 200 OK</c> if request was successful.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Delete(int id)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(id);
            if (campaign == null)
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (campaign.CampaignSiteID == SiteContext.CurrentSiteID)
            {
                CampaignInfoProvider.DeleteCampaignInfo(campaign);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        /// <summary>
        /// Updates UTM campaign codes in newsletters.
        /// </summary>
        /// <param name="oldUTMCode">Original UTM campaign code.</param>
        /// <param name="newUTMCode">New UTM campaign code.</param>
        private void UpdateNewsletterUTMCodes(string oldUTMCode, string newUTMCode)
        {
            var q = new DataQuery(PredefinedObjectType.NEWSLETTERISSUE, QueryName.GENERALUPDATE).WhereEquals("IssueUTMCampaign", oldUTMCode);

            var parameters = new UpdateQueryExpression(new Dictionary<string, object> { { "IssueUTMCampaign", newUTMCode } });
            var valuesExpression = q.IncludeDataParameters(parameters.Parameters, parameters.GetExpression());
            q.EnsureParameters().AddMacro(QueryMacros.VALUES, valuesExpression);

            q.Execute();

            ProviderHelper.ClearHashtables(PredefinedObjectType.NEWSLETTERISSUE, true);
        }


        private static void CheckCampaignExists(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }



        private static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, ModelStateDictionary modelState)
        {
            var message = modelState["model.UTMCode"] == null
                    ? modelState.ToString()
                    : modelState["model.UTMCode"].Errors.First().ErrorMessage;

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(message)
            };
        }
    }
}
