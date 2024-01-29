using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebAnalytics.Web.UI;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(CampaignConversionPageController))]

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Supplies data for page Smart Drop-down in campaign conversions.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class CampaignConversionPageController : CMSApiController, ISelectorController<PageSelectorViewModel>
    {
        private const string EVENT_TYPE = "EVENT";
        private const string PAGE_TYPE = "PAGE";
        private const string PRODUCT_TYPE = "PRODUCT";

        private readonly string[] mExcludedClasses = { SystemDocumentTypes.Root, SystemDocumentTypes.Folder };


        /// <summary>
        /// Gets page according to <paramref name="ID"/> and <paramref name="objType"/>.
        /// </summary>
        /// <param name="objType">Object type.</param>
        /// <param name="ID">Page node ID or product node SKUID.</param>        
        /// <returns>
        /// <c>HTTP status code 400 Bad Request</c>, if page is not found;
        /// otherwise, <c>HTTP status code 200 OK</c> with serialized <see cref="PageSelectorViewModel"/> view model.
        /// </returns>
        public PageSelectorViewModel Get(string objType, int ID)
        {
            CheckObjectTypeValidity(objType);

            TreeNode document;

            if (IsPageType(objType))
            {
                document = DocumentHelper.GetDocument(ID, LocalizationContext.PreferredCultureCode, true, null);
            }
            else if (IsEventType(objType))
            {
                document = DocumentHelper.GetDocument(ID, null);
            }
            else
            {
                document = DocumentHelper.GetDocuments()
                                         .Columns("NodeSKUID", "NodeSiteID", "DocumentName", "DocumentNamePath", "ClassName")
                                         .WhereEquals("NodeSKUID", ID)
                                         .OnSite(SiteContext.CurrentSiteName)
                                         .Culture(LocalizationContext.PreferredCultureCode)
                                         .CombineWithDefaultCulture()
                                         .FirstOrDefault();
            }

            if (IsInvalidResult(document, objType))
            {
                ThrowBadRequest("Node with selected ID was not found.");
            }

            return CreatePageViewModel(document, objType);
        }

        
        /// <summary>
        /// Gets list of pages for current site, where current user has read permission.
        /// </summary>
        /// <param name="objType">Object type.</param>
        /// <param name="name">Search query (part of page's name).</param>        
        /// <param name="pageIndex">Index of the page. Pages are indexed from 0 (first page).</param>
        /// <param name="pageSize">Number of results in the page.</param>
        /// <returns>
        /// List of 10 serialized <see cref="PageSelectorViewModel"/> view models.
        /// </returns>
        public IEnumerable<PageSelectorViewModel> Get(string objType, string name = "", int pageIndex = 0, int pageSize = 10)
        {
            CheckObjectTypeValidity(objType);
            var pages = GetDocumentsFor(objType)
                                      .WhereContains("DocumentName", name)
                                      .Page(pageIndex, pageSize)
                                      .CheckPermissions()
                                      .ToList();

            return pages.Select(d => CreatePageViewModel(d, objType));
        }


        private bool IsInvalidResult(TreeNode page, string objType)
        {
            var isInvalidPage = (page == null) 
                || page.IsRoot() || page.IsFolder()
                || (page.NodeSiteID != SiteContext.CurrentSiteID)
                || !page.CheckPermissions(PermissionsEnum.Read, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);

            return (IsPageType(objType) || IsEventType(objType))
                ? isInvalidPage
                : isInvalidPage || page.NodeSKUID == 0;
        }


        private void CheckObjectTypeValidity(string objType)
        {
            if (!(IsPageType(objType) || IsProductType(objType) || IsEventType(objType)))
            {
                ThrowBadRequest("Invalid object type.");
            }
        }


        private bool IsPageType(string objType)
        {
            return string.Equals(PAGE_TYPE, objType, StringComparison.OrdinalIgnoreCase);
        }


        private bool IsProductType(string objType)
        {
            return string.Equals(PRODUCT_TYPE, objType, StringComparison.OrdinalIgnoreCase);
        }


        private bool IsEventType(string objType)
        {
            return string.Equals(EVENT_TYPE, objType, StringComparison.OrdinalIgnoreCase);
        }


        private string GetIcon(TreeNode node)
        {
            var dataClass = DataClassInfoProvider.GetDataClassInfo(node.ClassName);
            if (dataClass != null)
            {
                var iconClass = dataClass.GetStringValue("ClassIconClass", string.Empty);
                return UIHelper.GetDocumentTypeIcon(null, node.DocumentType, iconClass);
            }

            return string.Empty;            
        }


        private PageSelectorViewModel CreatePageViewModel(TreeNode node, string objType)
        {
            return new PageSelectorViewModel
            {
                ID = GetPageId(node, objType),
                Text = node.DocumentName,
                Path = node.DocumentNamePath,
                Icon = GetIcon(node)
            };
        }


        private int GetPageId(TreeNode node, string objType)
        {
            if (IsPageType(objType))
            {
                return node.NodeID;
            }
            if (IsEventType(objType))
            {
                return node.DocumentID;
            }
            return node.NodeSKUID;
        }


        private static void ThrowBadRequest(string message)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(message)
            });
        }


        private MultiDocumentQuery GetDocumentsFor(string objType)
        {
            var query = DocumentHelper.GetDocuments()
                                      .Columns("NodeID", "NodeSKUID", "NodeSiteID", "DocumentName", "DocumentNamePath", "ClassName", "DocumentID")
                                      .WhereNotIn("ClassName", mExcludedClasses)
                                      .OnSite(SiteContext.CurrentSiteName)
                                      .Culture(LocalizationContext.PreferredCultureCode)
                                      .CombineWithDefaultCulture()
                                      .OrderBy("DocumentName");

            if (IsEventType(objType))
            {
                query.WhereEquals("ClassName", "cms.bookingevent");
            }
            else if (IsProductType(objType))
            {
                // Remove pages with duplicate NodeSKUID
                query.WhereIn("NodeID", 
                    new ObjectQuery(PredefinedObjectType.NODE)
                        .Column(new AggregatedColumn(AggregationType.Min, "NodeID"))
                        .OnSite(SiteContext.CurrentSiteName)
                        .WhereNotNull("NodeSKUID")
                        .GroupBy("NodeSKUID")
                );
            }

            return query;
        }
    }
}
