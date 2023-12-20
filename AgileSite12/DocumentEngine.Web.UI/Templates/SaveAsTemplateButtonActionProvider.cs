using System;

using CMS.Core;
using CMS.Base.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Modules;

namespace CMS.DocumentEngine.Web.UI.Internal
{
    /// <summary>
    /// Provides the button action for the 'Save as template' MVC feature.
    /// </summary>
    public class SaveAsTemplateButtonActionProvider
    {
        /// <summary>
        /// Gets the button action.
        /// </summary>
        /// <returns>Returns <c>null</c> if the action should not be available in the current context.</returns>
        public CMSButtonAction Get()
        {
            if (!SiteContext.CurrentSite.SiteIsContentOnly || !PortalContext.ViewMode.IsEdit())
            {
                return null;
            }

            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.Content", "ManagePageTemplates"))
            {
                return null;
            }

            var page = DocumentContext.EditedDocument;
            if (String.IsNullOrEmpty(page.GetStringValue("DocumentPageTemplateConfiguration", null)))
            {
                return null;
            }

            return new CMSButtonAction
            {
                Text = CoreServices.Localization.GetString("pagetemplatesmvc.saveastemplate"),
                OnClientClick = GetOnClickScript(),
                ToolTip = CoreServices.Localization.GetString("pagetemplatesmvc.saveastemplate.tooltip")
            };
        }


        private static string GetOnClickScript()
        {
            var modalDialogUrl = ApplicationUrlHelper.GetElementDialogUrl(
                ModuleName.CONTENT,
                "Page.SaveAsTemplate",
                additionalQuery: $"parentobjectid={DocumentContext.EditedDocument.DocumentID}"
            );

            return
                $@"if(CMSContentManager.contentModified()) {{
                    alert('{CoreServices.Localization.GetString("pagetemplatesmvc.unsavedchanges")}');
                    return false;
                }}
                else {{
                    modalDialog('{ modalDialogUrl }', 'Page.TemplatePropertiesMVC', 750, '50%');
                    return false;
                }}";
        }
    }
}
