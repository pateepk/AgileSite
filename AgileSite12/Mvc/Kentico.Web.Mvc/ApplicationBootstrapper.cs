using System;
using System.Web;
using System.Web.Hosting;
using System.Configuration;

using CMS.Base;
using CMS.DataEngine;
using CMS.IO;


[assembly: PreApplicationStartMethod(typeof(Kentico.Web.Mvc.ApplicationBootstrapper), "Run")]

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Initializes Kentico integration with ASP.NET MVC. This class is for internal use only.
    /// </summary>
    public static class ApplicationBootstrapper
    {
        private static bool AllowAutoInit
        {
            get
            {
                if (Boolean.TryParse(ConfigurationManager.AppSettings["CmsMvcAutoAppInit"], out bool result))
                {
                    return result;
                }

                return true;
            }
        }


        /// <summary>
        /// Runs the bootstrapper process.
        /// </summary>
        public static void Run()
        {
            SystemContext.IsWebSite = true;
            VirtualPathHelper.VirtualPathProviderAllowedByDefault = false;

            // Do not initialize application in ClientBuildManager domains (e.g aspnet_compiler.exe or visual studio compiler)
            if (HostingEnvironment.InClientBuildManager)
            {
                return;
            }

            CMSApplication.PreInit();

            if (AllowAutoInit)
            {
                CMSApplication.Init();
            }
        }
    }
}
