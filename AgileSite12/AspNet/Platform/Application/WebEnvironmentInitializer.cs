using System;
using System.Web;

using CMS.AspNet.Platform;
using CMS.Base;

[assembly: PreApplicationStartMethod(typeof(WebEnvironmentInitializer), "SetUp")]

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Class is responsible for initializing website environment.
    /// </summary>
    public static class WebEnvironmentInitializer
    {
        /// <summary>
        /// Methods sets properties <see cref="SystemContext.IsWebSite"/>, <see cref="SystemContext.ApplicationPath"/> and <see cref="SystemContext.WebApplicationPhysicalPath"/> by values provided by <see cref="HttpRuntime"/> implementation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The method is not intended to be used directly from your code.
        /// </para>
        /// <para>
        /// Method is called automatically during the application startup for applications hosted within the web server.
        /// </para>
        /// </remarks>
        [Obsolete("This code is meant for system purposes, it shouldn't be used directly.", true)]
        public static void SetUp()
        {
            SystemContext.IsWebSite = true;
            SystemContext.ApplicationPath = HttpRuntime.AppDomainAppVirtualPath;
            SystemContext.WebApplicationPhysicalPath = HttpRuntime.AppDomainAppPath.TrimEnd('\\');
        }
    }
}
