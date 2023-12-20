using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.SiteProvider;

[assembly: RegisterModule(typeof(SiteModule))]

namespace CMS.SiteProvider
{
    /// <summary>
    /// Represents the Site module.
    /// </summary>
    public class SiteModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SiteModule()
            : base(new SiteModuleMetadata())
        {
        }


        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterContext<SiteContext>("SiteContext");

            SiteLocalizationContext.Init();

            SiteHandlers.Init();

            InitMacros();
        }

        
        /// <summary>
        /// Initializes the macro engine
        /// </summary>
        private static void InitMacros()
        {
            MacroContext.GlobalResolver.AddSourceAlias("Site", "SiteContext.CurrentSite");
        }
        

        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            CultureSiteInfoProvider.ClearSiteCultures(logTasks);
            SiteInfo.TYPEINFO.InvalidateAllObjects(logTasks);
        }
    }
}