using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Core;
using CMS.Helpers;
using CMS.WebAnalytics.Web.UI;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(CampaignObjectiveController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Handles the campaign objectives.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.WEBANALYTICS, "ManageCampaigns")]
    [HandleExceptions]
    public sealed class CampaignObjectiveController : CMSApiController
    {
        private readonly ICampaignObjectiveService mObjectiveService = Service.Resolve<ICampaignObjectiveService>();


        /// <summary>
        /// Updates existing or creates a new instance of the <see cref="CampaignObjectiveInfo"/> corresponding to the given <see cref="CampaignObjectiveViewModel"/>.
        /// The <see cref="CampaignObjectiveInfo"/> is updated or inserted into database with the values obtained from <paramref name="objectiveViewModel"/>.
        /// </summary>
        /// <param name="objectiveViewModel">Instance of view model with objective information. This instance will be used to update or create corresponding <see cref="CampaignObjectiveInfo"/></param>
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if model state of bound <paramref name="objectiveViewModel"/> is not valid;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <paramref name="objectiveViewModel"/>.
        /// </returns>
        [HttpPost]
        public CampaignObjectiveViewModel Post([FromBody] CampaignObjectiveViewModel objectiveViewModel)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var campaignID = objectiveViewModel.CampaignID;

            if (CampaignIsFinished(campaignID))
            {
                var message = ResHelper.GetString("campaign.objective.modifyobjective");
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }

            var existingObjective = GetCampaignObjective(campaignID);

            if (CampaignHasObjective(existingObjective, objectiveViewModel))
            {
                throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, ResHelper.GetString("campaign.objective.alreadyexist")));
            }

            // New objective is being created -> campaign does not have any objective yet
            if (existingObjective == null)
            {
                return mObjectiveService.SaveObjective(objectiveViewModel);
            }

            // Update existing objective
            return UpdateObjective(objectiveViewModel, existingObjective);
        }


        /// <summary>
        /// Deletes the given campaign objective.
        /// </summary>
        /// <remarks>
        /// When response is returned, there will be no objective with given ID in the database.
        /// HTTP verb DELETE is not supported, therefore this request accepts only GET requests.
        /// </remarks>
        /// <param name="objectiveID">ID of objective</param>
        /// <returns>
        /// Returns <c>HTTP status code 200 OK</c> if request is successful.
        /// </returns>
        [HttpPost]
        public HttpResponseMessage Delete(int objectiveID)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            var objective = CampaignObjectiveInfoProvider.GetCampaignObjectiveInfo(objectiveID);
            if (objective != null)
            {
                var campaign = CampaignInfoProvider.GetCampaignInfo(objective.CampaignObjectiveCampaignID);
                var status = campaign.GetCampaignStatus(DateTime.Now);

                if (status == CampaignStatusEnum.Finished)
                {
                    var message = ResHelper.GetString("campaign.objective.removefinished");
                    throw new HttpResponseException(CreateErrorResponse(HttpStatusCode.BadRequest, message));
                }

                CampaignObjectiveInfoProvider.DeleteCampaignObjectiveInfo(objective);
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


        private bool CampaignIsFinished(int campaignID)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignID);

            return campaign.GetCampaignStatus(DateTime.Now) == CampaignStatusEnum.Finished;
        }


        private CampaignObjectiveInfo GetCampaignObjective(int campaignID)
        {
            return CampaignObjectiveInfoProvider.GetCampaignObjectives()
                .WhereEquals("CampaignObjectiveCampaignID", campaignID)
                .TopN(1)
                .FirstOrDefault();
        }


        private bool ObjectiveIsSame(CampaignObjectiveInfo objective, CampaignObjectiveViewModel model)
        {
            return (objective.CampaignObjectiveValue == model.Value) && (objective.CampaignObjectiveCampaignConversionID == model.ConversionID);
        }


        private CampaignObjectiveViewModel UpdateObjective(CampaignObjectiveViewModel objectiveViewModel, CampaignObjectiveInfo campaignObjective)
        {
            // There is nothing to be updated
            if (ObjectiveIsSame(campaignObjective, objectiveViewModel))
            {
                return objectiveViewModel;
            }

            return mObjectiveService.SaveObjective(objectiveViewModel);
        }


        private bool CampaignHasObjective(CampaignObjectiveInfo existingObjective, CampaignObjectiveViewModel objectiveViewModel)
        {
            return (existingObjective != null) && (existingObjective.CampaignObjectiveID != objectiveViewModel.ID);
        }
    }
}
