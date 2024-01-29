using System;

using CMS;
using CMS.Base;
using CMS.CMSImportExport;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.Synchronization;

[assembly: RegisterModule(typeof(PortalModule))]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Represents the Portal module.
    /// </summary>
    public class PortalModule : Module
    {
        #region "Constants"

        internal const string WEBPART = "##WEBPART##";
        internal const string WIDGET = "##WIDGET##";
        internal const string PAGETEMPLATE = "##PAGETEMPLATE##";

        #endregion
        

        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        public PortalModule()
            : base(new PortalModuleMetadata())
        {
        }


        /// <summary>
        /// Pre-initializes the module
        /// </summary>
        protected override void OnPreInit()
        {
            Service.Use<ISiteService, SiteService>();
        }


        /// <summary>
        /// Init module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterVirtualPaths();

            RegisterContext<PortalContext>("PortalContext");
            RegisterContext<SystemContext>("SystemContext");

            InitMacros();
            RegisterMacroMethods();

            // Import export handlers
            InitImportExport();

            InitSynchronization();

            PortalHandlers.Init();

            // Sets the CMSTransformation define in AppCode as a transformation base class within main application
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                TransformationInfoProvider.TransformationBaseClass = "CMS.DocumentEngine.Web.UI.CMSTransformation";
            }
        }


        /// <summary>
        /// Excludes set of hidden system keys from synchronization.
        /// </summary>
        private static void InitSynchronization()
        {
            // Exclude those system hidden keys from synchronization
            SynchronizationHelper.AddExcludedSettingKey(
                "CMSStoreWebpartContainersInFS",
                "CMSStoreCSSStylesheetsInFS",
                "CMSStoreLayoutsInFS",
                "CMSStorePageTemplatesInFS",
                "CMSStoreTransformationsInFS",
                "CMSStoreWebPartLayoutsInFS",
                "CMSDeploymentMode",
                "CMSDataVersion",
                "CMSDBVersion"
            );
        }


        /// <summary>
        /// Initializes import/export handlers
        /// </summary>
        private static void InitImportExport()
        {
            // Exclude those system hidden keys from synchronization
            ImportExportHelper.AddExcludedSettingKey(
                "CMSStoreWebpartContainersInFS",
                "CMSStoreCSSStylesheetsInFS",
                "CMSStoreLayoutsInFS",
                "CMSStorePageTemplatesInFS",
                "CMSStoreTransformationsInFS",
                "CMSStoreWebPartLayoutsInFS",
                "CMSDeploymentMode",
                "CMSDataVersion",
                "CMSDBVersion"
            );

            PageTemplateExport.Init();
            PageTemplateImport.Init();
            WebPartExport.Init();
            WebPartImport.Init();
            WidgetExport.Init();
            WidgetImport.Init();
            CssStylesheetImport.Init();
            CssStylesheetExport.Init();
            PageLayoutImport.Init();
            ImportSpecialActions.Init();
            ExportSpecialActions.Init();
        }


        /// <summary>
        /// Initializes the Portal engine macros
        /// </summary>
        private static void InitMacros()
        {
            var r = MacroContext.GlobalResolver;

            r.SetNamedSourceDataCallback("ViewMode", x => PortalContext.ViewMode.ToString(), false);
        }


        /// <summary>
        /// Registers macro methods.
        /// </summary>
        private static void RegisterMacroMethods()
        {
            MacroMethod met = new MacroMethod("CssPreprocessorsDatasourceOptions", CssPreprocessorsDatasourceOptions)
            {
                Comment = "Return list of registered CSS preprocessors in format <value>;<name> usable e.g. as a data source of form controls.",
                Type = typeof(string),
                IsHidden = true
            };

            MacroMethods.RegisterMethod(met);

            met = new MacroMethod("CssPreprocessorDisplayName", CssPreprocessorDisplayName)
            {
                Comment = "Returns string that represents the display name of CSS preprocessor given by an argument.",
                Type = typeof(string),
                IsHidden = true
            };

            MacroMethods.RegisterMethod(met);            
        }
        

        /// <summary>
        /// The implementation of CssPreprocessorsDatasourceOptions macro method. Return list of value;name separated by EOL character.
        /// </summary>
        private static object CssPreprocessorsDatasourceOptions(params object[] parameters)
        {
            string result = "";

            foreach (CssPreprocessor item in CssStylesheetInfoProvider.CssPreprocessors.Values)
            {
                result += item.Name + ";" + item.DisplayName + "\n";
            }

            return result;
        }


        /// <summary>
        /// The implementation of CssPreprocessorName macro method. Returns string that represents the display name of CSS preprocessor given by an argument.
        /// </summary>
        private static object CssPreprocessorDisplayName(params object[] parameters)
        {
            if (parameters.Length == 1)
            {
                CssPreprocessor p = CssStylesheetInfoProvider.GetCssPreprocessor(parameters[0] as string);
                if (p != null)
                {
                    return p.DisplayName;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Registers the virtual paths to the virtual path provider
        /// </summary>
        private static void RegisterVirtualPaths()
        {
            // Transformation virtual object
            VirtualPathHelper.RegisterVirtualPath(TransformationInfoProvider.TransformationsDirectory, TransformationVirtualFileObject.GetVirtualFileObject);

            // AdHoc/Page template layout virtual object
            VirtualPathHelper.RegisterVirtualPath(PageTemplateInfoProvider.TemplateLayoutsDirectory, PageTemplateVirtualFileObject.GetVirtualFileObject);
            VirtualPathHelper.RegisterVirtualPath(PageTemplateInfoProvider.AdhocTemplateLayoutsDirectory, PageTemplateVirtualFileObject.GetAdHocVirtualFileObject);

            // AdHoc/Device layout virtual object
            VirtualPathHelper.RegisterVirtualPath(PageTemplateDeviceLayoutInfoProvider.DeviceLayoutsDirectory, DeviceLayoutVirtualFileObject.GetVirtualFileObject);
            VirtualPathHelper.RegisterVirtualPath(PageTemplateDeviceLayoutInfoProvider.AdHocDeviceLayoutsDirectory, DeviceLayoutVirtualFileObject.GetAdHocVirtualFileObject);

            // Layout virtual object
            VirtualPathHelper.RegisterVirtualPath(LayoutInfoProvider.LayoutsDirectory, LayoutVirtualFileObject.GetVirtualFileObject);

            // Web part layout virtual object
            VirtualPathHelper.RegisterVirtualPath(WebPartLayoutInfoProvider.WebPartLayoutsDirectory, WebPartLayoutVirtualFileObject.GetVirtualFileObject);

            // Virtual web part virtual object
            VirtualPathHelper.RegisterVirtualPath(WebPartInfoProvider.VirtualWebPartsDirectory, VirtualWebPartVirtualFileObject.GetVirtualFileObject);
        }


        /// <summary>
        /// Clears the module hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            PortalFormHelper.Clear(logTasks);
        }

        #endregion
    }
}