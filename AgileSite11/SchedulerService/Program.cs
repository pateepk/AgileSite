using System;
using System.IO;
using System.ServiceProcess;

using CMS.Base;
using CMS.WinServiceEngine;
using CMS.Core;

namespace CMS.SchedulerService
{
    /// <summary>
    /// Entry class.
    /// </summary>
    internal static class Program
    {
        #region "Methods"

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    string webAppPath = null;

                    // Read parameter 'webpath'
                    foreach (string arg in args)
                    {
                        string argument = arg.ToLowerCSafe().TrimStart('/').TrimEnd('\\');

                        // 'Webpath' parameter
                        if (argument.StartsWithCSafe(ServiceHelper.WEB_PATH_PREFIX))
                        {
                            webAppPath = argument.Replace(ServiceHelper.WEB_PATH_PREFIX, string.Empty);
                        }
                    }

                    if (!string.IsNullOrEmpty(webAppPath))
                    {
                        // Run Scheduler service
                        if (args.Length == 1)
                        {
                            // Check web application directory
                            ServiceHelper.CheckWebApplicationPath(webAppPath);

                            // Configure the application core.
#pragma warning disable BH1014 // Do not use System.IO
                            var dependenciesPath = Path.Combine(webAppPath, "CMSDependencies");
#pragma warning restore BH1014 // Do not use System.IO

                            var setup = new AppCoreSetup.Builder()
                                .WithDependenciesFolderPath(dependenciesPath)
                                .Build();

                            AppCore.Setup(setup);
                            AppCore.PreInit();

                            var servicesToRun = new ServiceBase[] { new SchedulerService(webAppPath) };

                            ServiceBase.Run(servicesToRun);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceHelper.LogException(WinServiceHelper.SCHEDULER_SERVICE_BASENAME, ex, true);
            }
        }

        #endregion
    }
}