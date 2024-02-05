using System;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.UIControls.Extenders;

[assembly: RegisterExtender(typeof(PortalManagerHeaderPanelBreadcrumbsExtender))]

namespace CMS.UIControls.Extenders
{
    /// <summary>
    /// Extender for extending main header panel in Portal manager with breadcrumbs control.
    /// </summary>
    public sealed class PortalManagerHeaderPanelBreadcrumbsExtender : Extender<IExtendableHeaderPanel>
    {
        /// <summary>
        /// Initializes a new instance of the PanelHeaderExtender class.
        /// </summary>
        public PortalManagerHeaderPanelBreadcrumbsExtender()
            : base("PortalManager")
        {

        }


        /// <summary>
        /// Adds a controls to Portal manager header panel.
        /// </summary>
        /// <param name="headerPanel">The panel to add additional controls.</param>
        protected override void Initialize(IExtendableHeaderPanel headerPanel)
        {
            if (!PortalContext.ViewMode.IsPreview())
            {
                return;
            }

            if (QueryHelper.GetBoolean("showbreadcrumbs", false))
            {
                try
                {
                    var ctrBreadcrumbs = PageContext.CurrentPage.LoadControl("~/CMSAdminControls/UI/PageElements/BreadCrumbs.ascx") as Breadcrumbs;
                    if (ctrBreadcrumbs != null)
                    {
                        ctrBreadcrumbs.ID = "ctrBreadcrumbs";

                        // Insert breadcrumb data
                        ctrBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
                        {
                            Text = SiteContext.CurrentSiteName
                        });

                        ctrBreadcrumbs.AddBreadcrumb(new BreadcrumbItem
                        {
                            Text = headerPanel.CurrentNode.GetDocumentName()
                        });

                        CMSPanel panel = new CMSPanel
                        {
                            ID = "pnlBreadcrumbs",
                            ShortID = "pb",
                            CssClass = "preview-breadcrumbs"
                        };

                        panel.Controls.Add(ctrBreadcrumbs);

                        headerPanel.AddAdditionalControl(panel);
                    }
                }
                catch (Exception exception)
                {
                    EventLogProvider.LogException("UI.Controls", "ExtenderFailed", exception, additionalMessage: "There was an exception extending the specified target.");
                }
            }
        }
    }
}
