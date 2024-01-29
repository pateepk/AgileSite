using System.Web.Routing;

using CMS.Base;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Web site context methods and variables.
    /// </summary>
    internal class CMSContext : AbstractContext<CMSContext>
    {
        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<CMSDataContext>("Current", m => CMSDataContext.Current);

            RegisterProperty<CurrentUserInfo>("GlobalPublicUser", m => AuthenticationHelper.GlobalPublicUser);
            RegisterProperty<CurrentUserInfo>("CurrentUser", m => MembershipContext.AuthenticatedUser);
            RegisterProperty<SiteInfo>("CurrentSite", m => SiteContext.CurrentSite);

            RegisterProperty<TreeNode>("CurrentDocument", m => DocumentContext.CurrentDocument);
            RegisterProperty<TreeNode>("CurrentDocumentParent", m => DocumentContext.CurrentDocumentParent);
            RegisterProperty<CultureInfo>("CurrentDocumentCulture", m => DocumentContext.CurrentDocumentCulture);
            RegisterProperty<UserInfo>("CurrentDocumentOwner", m => DocumentContext.CurrentDocumentOwner);
            RegisterProperty<AttachmentInfo>("CurrentAttachment", m => DocumentContext.CurrentAttachment);
            RegisterProperty<PageInfo>("CurrentPageInfo", m => DocumentContext.CurrentPageInfo);
            RegisterProperty<PageTemplateInfo>("CurrentTemplate", m => DocumentContext.CurrentTemplate);

            RegisterProperty<CssStylesheetInfo>("CurrentDocumentStylesheet", m => DocumentContext.CurrentDocumentStylesheet);
            RegisterProperty<CssStylesheetInfo>("CurrentSiteStylesheet", m => PortalContext.CurrentSiteStylesheet);

            RegisterProperty("CurrentDiscussionResolver", m => DiscussionMacroHelper.CurrentDiscussionResolver);
            RegisterProperty("CurrentResolver", m => MacroContext.CurrentResolver);
            RegisterProperty("CurrentDevice", m => DeviceContext.CurrentDevice);

            RegisterProperty<RouteData>("CurrentRouteData", m => ModuleManager.GetContextProperty("URLRewritingContext", "CurrentRouteData"));
            RegisterProperty<TreeNode>("CurrentDepartment", m => ModuleManager.GetContextProperty("CommunityContext", "CurrentDepartment"));
        }


        /// <summary>
        /// Registers the Columns of this object
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            RegisterColumn("CurrentAliasPath", m => DocumentContext.CurrentAliasPath);
            RegisterColumn("CurrentBodyClass", m => DocumentContext.CurrentBodyClass);
            RegisterColumn("CurrentDescription", m => DocumentContext.CurrentDescription);
            RegisterColumn("CurrentDocType", m => DocumentContext.CurrentDocType);
            RegisterColumn("CurrentKeyWords", m => DocumentContext.CurrentKeyWords);
            RegisterColumn("CurrentTitle", m => DocumentContext.CurrentTitle);
            RegisterColumn("CurrentDocumentStylesheetName", m => DocumentContext.CurrentDocumentStylesheetName);
            RegisterColumn("CurrentSiteStylesheetName", m => PortalContext.CurrentSiteStylesheetName);
            RegisterColumn("CurrentSiteName", m => SiteContext.CurrentSiteName);
            RegisterColumn<int>("CurrentSiteID", m => SiteContext.CurrentSiteID);

            RegisterColumn("CurrentVisitStatus", m => ModuleManager.GetContextProperty("AnalyticsContext", "CurrentVisitStatus"));
        }
    }
}
