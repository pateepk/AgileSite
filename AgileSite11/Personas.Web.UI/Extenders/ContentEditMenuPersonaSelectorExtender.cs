using System;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Personas.Web.UI;
using CMS.PortalEngine;

[assembly: RegisterExtender(typeof(ContentEditMenuPersonaSelectorExtender))]

namespace CMS.Personas.Web.UI
{
    /// <summary>
    /// Adds a persona selector to the edit menu of the Content module.
    /// </summary>
    internal sealed class ContentEditMenuPersonaSelectorExtender : Extender<IExtensibleEditMenu>
    {
        /// <summary>
        /// Initializes a new instance of the ContentEditMenuPersonaSelectorExtender class.
        /// </summary>
        public ContentEditMenuPersonaSelectorExtender() : base("Content")
        {
        }


        /// <summary>
        /// Adds a persona selector to the edit menu of the Content module.
        /// </summary>
        /// <param name="menu">The menu to add the persona selector to.</param>
        protected override void Initialize(IExtensibleEditMenu menu)
        {
            if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.Personas))
            {
                return;
            }

            if (!PortalContext.ViewMode.IsPreview())
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
                menu.AddAdditionalControl(new PreviewModePersonasSelector());
            }
            catch (Exception exception)
            {
                EventLogProvider.LogException("ContentEditMenuPersonaSelectorExtender", "ExtenderFailed", 
                    exception, additionalMessage: "There was an exception extending the specified target.");
            }
        }
    }
}