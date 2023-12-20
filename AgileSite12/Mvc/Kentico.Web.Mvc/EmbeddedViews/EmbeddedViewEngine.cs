using System;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;

namespace Kentico.Web.Mvc
{
    internal class EmbeddedViewEngine : VirtualPathProviderViewEngine
    {
        private readonly EmbeddedViewMapping mappingTable;

        public EmbeddedViewEngine()
            : this(EmbeddedViewMapping.Instance)
        {
        }


        internal EmbeddedViewEngine(EmbeddedViewMapping embeddedViewMapping)
        {
            SetupLocations();
            mappingTable = embeddedViewMapping;
        }


        private void SetupLocations()
        {
            ViewLocationFormats = new[]
            {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
            };

            AreaViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };

            // File extensions not needed because of unsupported start page functionality (_ViewStart.cshtml file)
            // FileExtensions = new[] { "cshtml" };

            AreaMasterLocationFormats = AreaViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;

            PartialViewLocationFormats = ViewLocationFormats;
            MasterLocationFormats = ViewLocationFormats;
        }


        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            mappingTable.TryGetView(partialPath, out Type type);
            return new EmbeddedView(partialPath, type);
        }


        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            mappingTable.TryGetView(viewPath, out Type type);
            return new EmbeddedView(viewPath, type);
        }


        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return mappingTable.ContainsView(virtualPath);
        }


        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            ValidateViewNameFormat(viewName);

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }


        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            ValidateViewNameFormat(partialViewName);

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }


        internal static void ValidateViewNameFormat(string viewName)
        {
            if (SystemContext.DiagnosticLogging 
                && (!viewName.StartsWith("~/", StringComparison.Ordinal))
                && viewName.IndexOf("Kentico/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var eventLogService = Service.Resolve<IEventLogService>();
                eventLogService.LogEvent("E", "EmbeddedViewEngine", "VIEWPATH", $"{viewName} has incorrect format. VirtualPath of system views must start with ~/.");
            }
        }
    }
}
