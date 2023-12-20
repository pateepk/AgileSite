using System;
using System.Web;

using CMS.Base;
using CMS.IO;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Provides functions for application management.
    /// </summary>
    internal static class ApplicationRuntime
    {
        /// <summary>
        /// Attempts to restart an application using <see cref="HttpRuntime.UnloadAppDomain"/>.
        /// </summary>
        public static void Restart(RestartRequiredEventArgs eventArgs)
        {
            if (SystemContext.IsRunningOnAzure)
            {
                return;
            }

            var isRestarted = false;

            try
            {
                // Try to restart application by unload app domain
                HttpRuntime.UnloadAppDomain();

                isRestarted = true;
            }
            catch
            {
                try
                {
                    var path = Path.Combine(SystemContext.WebApplicationPhysicalPath, "web.config");

                    // Try to restart application by changing web.config file
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);

                    isRestarted = true;
                }
                catch
                {
                    // possible disk access error
                }
            }

            eventArgs.IsRestarted = isRestarted;
        }
    }
}
