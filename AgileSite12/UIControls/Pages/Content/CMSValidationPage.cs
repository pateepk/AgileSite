using System;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the properties pages to apply global settings to the pages
    /// </summary>
    public abstract class CMSValidationPage : CMSContentPage
    {
        #region "Properties"

        /// <summary>
        /// Validator element
        /// </summary>
        protected virtual DocumentValidator Validator
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            DocumentManager.RedirectForNonExistingDocument = false;
            base.OnPreInit(e);
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Check permissions for CMS Desk -> Content -> Properties tab
            CurrentUserInfo user = MembershipContext.AuthenticatedUser;
            if (!user.IsAuthorizedPerUIElement("CMS.Content", "Validation"))
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Validation");
            }
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (Validator != null)
            {
                Validator.NodeID = NodeID;
                Validator.CultureCode = CultureCode;
                Validator.Url = GetDocumentUrl();
            }
            base.OnLoad(e);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns document URL or redirect to page not available page 
        /// </summary>
        protected string GetDocumentUrl()
        {
            int nodeId = NodeID;
            string url = null;

            // Get the document
            TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            string siteName = SiteContext.CurrentSiteName;
            bool siteCombineWithDefaultCulture = SiteInfoProvider.CombineWithDefaultCulture(siteName);
            TreeNode node = tree.SelectSingleNode(nodeId, CultureCode, siteCombineWithDefaultCulture);

            // Redirect to the live URL
            if (node != null)
            {
                switch (ViewModeCode.FromString(Mode))
                {
                    case ViewModeEnum.LiveSite:
                        url = DocumentURLProvider.GetUrl(node);
                        url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.LiveSite).ToString());
                        break;

                    case ViewModeEnum.Preview:
                        // Use permanent URL to get proper preview mode
                        url = DocumentURLProvider.GetPermanentDocUrl(node.NodeGUID, node.NodeAlias, SiteContext.CurrentSiteName, PageInfoProvider.PREFIX_CMS_GETDOC, ".aspx");
                        url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName, CultureCode);
                        url = URLHelper.AddParameterToUrl(url, URLHelper.LanguageParameterName + ObjectLifeTimeFunctions.OBJECT_LIFE_TIME_KEY, "request");
                        url = URLHelper.AddParameterToUrl(url, "viewmode", ((int)ViewModeEnum.Preview).ToString());
                        url = URLHelper.AddParameterToUrl(url, "showpanel", false.ToString());
                        break;
                }
            }
            else
            {
                URLHelper.Redirect(DocumentUIHelper.GetPageNotAvailable(null, null));
            }

            return URLHelper.GetAbsoluteUrl(ResolveUrl(url));
        }

        #endregion
    }
}