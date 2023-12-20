using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Description;
using System.Web.Mvc;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs.Internal
{
    /// <summary>
    /// Represents Page selector dialog.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [VirtualContextAuthorize]
    public sealed class KenticoPageSelectorDialogController : Controller
    {
        private readonly PageSelectorPagesRepository pagesRepository;


        /// <summary>
        /// Creates an instance of the <see cref="KenticoPageSelectorDialogController"/> class.
        /// </summary>
        public KenticoPageSelectorDialogController()
            : this(new PageSelectorPagesRepository())
        {
        }


        /// <summary>
        /// Constructor for internal purposes.
        /// </summary>
        /// <param name="pagesRepository">Page selector repository instance.</param>
        internal KenticoPageSelectorDialogController(PageSelectorPagesRepository pagesRepository)
        {
            this.pagesRepository = pagesRepository ?? throw new ArgumentNullException(nameof(pagesRepository));
        }


        /// <summary>
        /// Renders the initial dialog iframe HTML.
        /// </summary>
        /// <param name="rootPath">The dialog root path.</param>
        [HttpGet]
        public ActionResult Index(string rootPath = "/")
        {
            var siteName = SiteContext.CurrentSiteName;
            var culture = CultureHelper.GetPreferredCulture();
            var dialogRootPage = pagesRepository.GetPage(rootPath, siteName, culture);
            if (dialogRootPage == null)
            {
                return GetErrorView(ResHelper.GetStringFormat("kentico.components.pageselector.invalidroot", rootPath));
            }

            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(dialogRootPage, NodePermissionsEnum.ExploreTree) == AuthorizationResultEnum.Denied)
            {
                return GetErrorView(ResHelper.GetStringFormat("kentico.components.pageselector.notauthorized", rootPath));
            }

            return View("~/Views/Shared/Kentico/Selectors/Dialogs/Pages/_Dialog.cshtml", new KenticoPageSelectorViewModel
            {
                SiteName = siteName,
                Culture = culture,
                GetChildPagesEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action(nameof(GetChildPages))).ToString(),
                GetAliasPathEndpointUrl = Url.Kentico().AuthenticateUrl(Url.Action(nameof(GetAliasPath))).ToString(),
                RootPage = PageSelectorItemModel.Create(dialogRootPage, Url),
            });
        }


        /// <summary>
        /// Returns child pages for the given <paramref name="nodeAliasPath"/>.
        /// </summary>
        /// <param name="nodeAliasPath">The node alias path.</param>
        [HttpGet]
        public ActionResult GetChildPages(string nodeAliasPath)
        {
            var siteName = SiteContext.CurrentSiteName;
            var culture = CultureHelper.GetPreferredCulture();
            var page = pagesRepository.GetPage(nodeAliasPath, siteName, culture);
            if (page == null)
            {
                return GetJsonResult(Enumerable.Empty<PageSelectorItemModel>());
            }

            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(page, NodePermissionsEnum.ExploreTree) == AuthorizationResultEnum.Denied)
            {
                return GetJsonResult(Enumerable.Empty<PageSelectorItemModel>());
            }

            var pages = pagesRepository.GetChildPages(page.NodeID, SiteContext.CurrentSiteName, CultureHelper.GetPreferredCulture());
            var model = pages.Select(p => PageSelectorItemModel.Create(p, Url));

            return GetJsonResult(model);
        }


        /// <summary>
        /// Returns a node alias path for given page GUID.
        /// </summary>
        /// <param name="pageGuid">Page GUID.</param>
        public ActionResult GetAliasPath(Guid pageGuid)
        {
            string aliasPath = string.Empty;

            if (pageGuid != Guid.Empty)
            {
                var page = pagesRepository.GetPage(pageGuid, SiteContext.CurrentSiteName, CultureHelper.GetPreferredCulture());
                aliasPath = (page != null) ? page.NodeAliasPath : aliasPath;
            }

            return GetJsonResult(aliasPath);
        }


        private ActionResult GetErrorView(string message)
        {
            var model = new KenticoDialogErrorViewModel
            {
                ErrorMessage = message
            };

            return View("~/Views/Shared/Kentico/Selectors/Dialogs/Shared/_Error.cshtml", model);
        }


        private ContentResult GetJsonResult(object model)
        {
            return Content(JsonConvert.SerializeObject(model), "application/json");
        }
    }
}
