using System;

using CMS;
using CMS.MacroEngine;
using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine.Web.UI;
using CMS.PortalEngine.Web.UI;

[assembly: RegisterModule(typeof(CMSPortalEngineWebUIModule))]

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Represents the Portal engine web UI module.
    /// </summary>
    internal class CMSPortalEngineWebUIModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CMSPortalEngineWebUIModule()
            : base(new CMSPortalEngineWebUIMetadata())
        {
        }


        #region "Methods"

        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            MacroContext.GlobalResolver.SetNamedSourceData(new MacroField("WebPartZone", () => WebPartZoneNamespace.Instance), false);
            MacroContext.GlobalResolver.SetNamedSourceData(new MacroField("WebPart", () => WebPartNamespace.Instance), false);

            MacroContext.GlobalResolver.SetNamedSourceData(new MacroField("DocumentWizard", () => new DocumentWizard()), false);

            // Beware that "Path" macro field is already registered to PathMacroContainer. PathNamespace cannot be registered under "Path" prefix. 
            MacroContext.GlobalResolver.AddAnonymousSourceData(PathNamespace.Instance);

            // Handle settings key changed
            SettingsKeyInfoProvider.OnSettingsKeyChanged += SettingsKeyInfoProvider_OnSettingsKeyChanged;

            base.OnInit();

            RegisterContext<UIContext>("UIContext");

            InitMacros();
        }


        /// <summary>
        /// Initializes the Portal engine macros
        /// </summary>
        private static void InitMacros()
        {
            var r = MacroContext.GlobalResolver;

            r.SetNamedSourceDataCallback("UIContext", x => UIContext.Current, false);
            r.SetNamedSourceDataCallback("EditedObject", x => UIContext.Current.EditedObject, false);
            r.SetNamedSourceDataCallback("EditedObjectParent", x => UIContext.Current.EditedObjectParent, false);
        }


        /// <summary>
        /// Settings key changed handler
        /// </summary>
        void SettingsKeyInfoProvider_OnSettingsKeyChanged(object sender, SettingsKeyChangedEventArgs e)
        {
            // Ensure cache item reload after change of setting key
            if (e.KeyName.EqualsCSafe("CMSPartialCacheItems"))
            {
                PartialCacheItemsProvider.ClearEnabledCacheItemsCache();
            }
        }

        #endregion
    }
}