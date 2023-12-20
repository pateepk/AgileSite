using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.Localization;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebAnalytics.Web.UI;
using CMS.WebAnalytics.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(CampaignConversionItemController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Provides conversion items for Smart Drop-down in campaign conversions. 
    /// Controller handles selected site-specific object types and checks read permissions. 
    /// </summary>
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class CampaignConversionItemController : CMSApiController, ISelectorController<BaseSelectorViewModel>
    {
        private readonly string[] mAllowedObjTypes = { PredefinedObjectType.NEWSLETTER, PredefinedObjectType.BIZFORM };
        private ICampaignConversionItemFilterContainer mCampaignConversionItemFilterContainer = Service.Resolve<ICampaignConversionItemFilterContainer>();


        /// <summary>
        /// Gets object according to <paramref name="ID"/> and it's <paramref name="objType"/>.
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <param name="ID">Object ID</param>
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if object is not found or insufficient permissions;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <see cref="BaseSelectorViewModel"/> view model.
        /// </returns>
        public BaseSelectorViewModel Get(string objType, int ID)
        {
            TryGetTypeInfo(objType);

            var infoObj = ProviderHelper.GetInfoById(objType, ID);

            if ((infoObj == null) || (infoObj.Generalized.ObjectSiteID != SiteContext.CurrentSiteID))
            {
                ThrowBadRequest();
            }

            return new BaseSelectorViewModel
            {
                ID = infoObj.Generalized.ObjectID,
                Text = ResHelper.LocalizeString(infoObj.Generalized.ObjectDisplayName, LocalizationContext.PreferredCultureCode)
            };
        }


        /// <summary>
        /// Gets collection of items on the current site. User must have read permission for given <paramref name="objType"/>.
        /// </summary>
        /// <param name="objType">Object type of requested items.</param>
        /// <param name="name">Search query (part of items's display name).</param>        
        /// <param name="pageIndex">Index of the page. Pages are indexed from 0 (first page).</param>
        /// <param name="pageSize">Number of results in the page.</param>
        /// <returns>
        /// Collection of 10 serialized <see cref="BaseSelectorViewModel"/> view models or
        /// <c>HTTP status code 400 Bad Request</c> if <paramref name="objType"/> is not supported or insufficient permissions.
        /// </returns>
        public IEnumerable<BaseSelectorViewModel> Get(string objType, string name = "", int pageIndex = 0, int pageSize = 10)
        {
            var typeInfo = TryGetTypeInfo(objType);

            var query = new ObjectQuery(objType)
                .OnSite(SiteContext.CurrentSiteID)
                .WhereContains(typeInfo.DisplayNameColumn, name)
                .Page(pageIndex, pageSize)
                .Columns(typeInfo.IDColumn, typeInfo.DisplayNameColumn)
                .OrderBy(typeInfo.DisplayNameColumn);

            query = TryToFilterQuery(query, objType);

            return query.ToList().Select(i => new BaseSelectorViewModel
            {
                ID = i.Generalized.ObjectID,
                Text = ResHelper.LocalizeString(i.Generalized.ObjectDisplayName, LocalizationContext.PreferredCultureCode)
            });
        }


        private ObjectQuery TryToFilterQuery(ObjectQuery objectQuery, string objectType)
        {
            var filter = mCampaignConversionItemFilterContainer.GetFilter(objectType);
            return filter != null ? filter.Filter(objectQuery) : objectQuery;
        }


        private ObjectTypeInfo TryGetTypeInfo(string objType)
        {
            if (!mAllowedObjTypes.Contains(objType))
            {
                ThrowBadRequest();
            }

            var infoObj = ModuleManager.GetObject(objType);

            if (infoObj == null)
            {
                ThrowBadRequest();
            }

            // Assign current site to empty object to correctly check permission for object read
            infoObj.Generalized.ObjectSiteID = SiteContext.CurrentSiteID;

            if (!infoObj.CheckPermissions(PermissionsEnum.Read, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser))
            {
                ThrowBadRequest();
            }

            return infoObj.TypeInfo;
        }


        private void ThrowBadRequest()
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Specified object is not found or you are not allowed to read the data.")
            });
        }
    }
}
