using System;

using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the editing for page template properties.
    /// </summary>
    public abstract class CMSEditTemplatePage : CMSModalDesignPage
    {
        #region "Variables"

        private int? mPageTemplateId = null;
        private PageTemplateInfo mPageTemplate = null;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Gets the page template ID from the query string "templateid".
        /// </summary>
        protected int PageTemplateID
        {
            get
            {
                if (mPageTemplateId == null)
                {
                    mPageTemplateId = QueryHelper.GetInteger("templateid", 0);
                }

                return mPageTemplateId.Value;
            }
        }


        /// <summary>
        /// Gets the page template info object (according to the PageTemplateID).
        /// </summary>
        protected PageTemplateInfo PageTemplate
        {
            get
            {
                if (mPageTemplate == null)
                {
                    mPageTemplate = PageTemplateInfoProvider.GetPageTemplateInfo(PageTemplateID);
                }

                return mPageTemplate;
            }
        }

        #endregion


        #region "Page methods"

        /// <summary>
        /// OnInit
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RequireSite = false;

            // Check permissions for CMS Desk -> Content -> Design -> Edit template
            var user = MembershipContext.AuthenticatedUser;
            if ((!user.IsAuthorizedPerUIElement("CMS.Design", "Design.EditTemplateProperties"))
                && (!user.IsAuthorizedPerUIElement("CMS.Design", "Template.EditProperties")))
            {
                RedirectToUIElementAccessDenied("CMS.Design", "Design.EditTemplateProperties");
            }

            // Check shared template
            
            bool sharedTemplate = false;
            if (PageTemplateID > 0)
            {
                if ((PageTemplate != null) && PageTemplate.IsReusable)
                {
                    sharedTemplate = true;
                }

                EditedObject = PageTemplate;
            }

            // User must be authorized to edit template either from properties or from design mode
            var currentUser = MembershipContext.AuthenticatedUser;
            string siteName = SiteContext.CurrentSiteName;

            bool authorizedForProperties = currentUser.IsAuthorizedPerUIElement("CMS.Content", new string[] { "Properties", "Properties.Template", "Template.EditProperties", (sharedTemplate ? "Template.ModifySharedTemplates" : null) }, siteName);
            bool authorizedForDesign = currentUser.IsAuthorizedPerUIElement("CMS.Design", new string[] { "Design", "Design.EditTemplateProperties", (sharedTemplate ? "Design.ModifySharedTemplates" : null) }, siteName);

            // If not authorized for either one, access denied to UI elements
            if (!authorizedForProperties && !authorizedForDesign)
            {
                RedirectToUIElementAccessDenied("CMS.Content", "Properties;Properties.Template;Template.EditProperties" + (sharedTemplate ? ";Template.ModifySharedTemplates" : null) + "|Design;Design.EditTemplateProperties" + (sharedTemplate ? ";Design.ModifySharedTemplates" : null));
            }
        }

        #endregion
    }
}