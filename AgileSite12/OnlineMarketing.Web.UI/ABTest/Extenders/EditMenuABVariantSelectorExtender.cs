using System;

using CMS;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.OnlineMarketing.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

[assembly: RegisterExtender(typeof(EditMenuABVariantSelectorExtender))]

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Adds an A/B variant selector combo button the edit menu of the Content module on MVC sites.
    /// </summary>
    internal class EditMenuABVariantSelectorExtender : Extender<IExtensibleEditMenu>
    {
        public EditMenuABVariantSelectorExtender() : base("Content")
        {
        }


        /// <summary>
        /// Adds an A/B variant selector combo button the edit menu of the Content module on MVC sites.
        /// </summary>
        /// <param name="instance">The menu to add the A/B variant selector to.</param>
        protected override void Initialize(IExtensibleEditMenu instance)
        {
            if (!SiteContext.CurrentSite.SiteIsContentOnly)
            {
                return;
            }

            if (!PortalContext.ViewMode.IsPreview() && !PortalContext.ViewMode.IsEdit() && !VirtualContext.IsInitialized)
            {
                return;
            }

            try
            {
                if (LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ABTesting))
                {
                    instance.AddAdditionalControl(new ABVariantSelector(PortalContext.ViewMode.IsPreview()));
                }
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("EditMenuABVariantSelectorExtender", "ExtenderFailed", exception, additionalMessage: "There was an exception extending the specified target.");
            }
        }
    }
}
