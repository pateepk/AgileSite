using System;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.UIControls.Extenders;

[assembly: RegisterExtender(typeof(EditMenuProfileSelectorExtender))]

namespace CMS.UIControls.Extenders
{
    /// <summary>
    /// Adds a profiles selector to the edit menu of the Content module.
    /// </summary>
    internal sealed class EditMenuProfileSelectorExtender : Extender<IExtensibleEditMenu>
    {
        /// <summary>
        /// Indicates if device selection button should be available
        /// </summary>
        internal bool ShowDeviceSelection
        {
            get
            {
                return QueryHelper.GetBoolean("showdevicesselection", !VirtualContext.IsPreviewLinkInitialized);
            }
        }


        /// <summary>
        /// Initializes a new instance of the EditMenuProfileSelectorExtender class.
        /// </summary>
        public EditMenuProfileSelectorExtender()
            : base("Content")
        {
        }


        /// <summary>
        /// Adds a profile selector to the edit menu of the Content module.
        /// </summary>
        /// <param name="menu">The menu to add the persona selector to.</param>
        protected override void Initialize(IExtensibleEditMenu menu)
        {
            if (SiteContext.CurrentSite.SiteIsContentOnly)
            {
                return;
            }

            if (!PortalContext.ViewMode.IsPreview()
                && !PortalContext.ViewMode.IsEdit(true)
                && !PortalContext.ViewMode.IsDesign(true))
            {
                return;
            }

            // Do nothing if document preview is displayed through preview link
            if (VirtualContext.IsPreviewLinkInitialized)
            {
                return;
            }

            try
            {
                // Show devices selection button
                if (!PortalUIHelper.DisplaySplitMode
                    && ShowDeviceSelection
                    && DeviceProfileInfoProvider.IsDeviceProfilesEnabled(SiteContext.CurrentSiteName)
                    && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.DeviceProfiles))
                {
                    var selector = PageContext.CurrentPage.LoadControl("~/CMSModules/DeviceProfiles/Controls/ProfilesMenuControl.ascx") as InlineUserControl;
                    if (selector != null)
                    {
                        selector.ID = "ucProfiles";
                        selector.SetValue("UseSmallButton", true);
                        menu.AddAdditionalControl(selector);
                    }
                }
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("EditMenuProfileSelectorExtender", "ExtenderFailed", exception, additionalMessage: "There was an exception extending the specified target.");
            }
        }
    }
}
