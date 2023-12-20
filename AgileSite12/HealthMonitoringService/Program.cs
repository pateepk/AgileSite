using System;
using System.IO;
using System.ServiceProcess;

using CMS.Core;
using CMS.Base;
using CMS.DataEngine;
using CMS.HealthMonitoring;
using CMS.WinServiceEngine;

namespace CMS.HealthMonitoringService
{
    /// <summary>
    /// Entry class.
    /// </summary>
    internal static class Program
    {
        #region "Constants"

        private const string USAGE = @"Usage: HealthMonitoringService.exe /webpath=<webpath> [/createcounters | /deletecounters] [/help]";
        private const string CREATE_COUNTERS_PARAM = "createcounters";
        private const string DELETE_COUNTERS_PARAM = "deletecounters";
        private const string HELP_PARAM = "help";

        #endregion


        #region "Methods"

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
                bool showUsage = true;

                if (args.Length > 0)
                {
                    string webAppPath = null;
                    bool createCounters = false;
                    bool deleteCounters = false;

                    // Read parameter 'webpath'
                    foreach (string arg in args)
                    {
                        string argument = arg.ToLowerCSafe().TrimStart('/').TrimEnd('\\');

                        // 'Help' parameter
                        if (argument == HELP_PARAM)
                        {
                            Console.WriteLine(USAGE);
                            return;
                        }
                        // 'Createcounters' parameter
                        else if (argument == CREATE_COUNTERS_PARAM)
                        {
                            createCounters = true;
                        }
                        // 'Deletecounters' parameter
                        else if (argument == DELETE_COUNTERS_PARAM)
                        {
                            deleteCounters = true;
                        }
                        // 'Webpath' parameter
                        else if (argument.StartsWithCSafe(ServiceHelper.WEB_PATH_PREFIX))
                        {
                            webAppPath = argument.Replace(ServiceHelper.WEB_PATH_PREFIX, string.Empty);
                        }
                    }

                    if (!string.IsNullOrEmpty(webAppPath))
                    {
                        // Check web application directory
                        ServiceHelper.CheckWebApplicationPath(webAppPath);

                        AppCore.PreInit();

                        // Run Health Monitoring service
                        if (!createCounters && !deleteCounters && (args.Length == 1))
                        {
                            showUsage = false;
                            ServiceBase[] ServicesToRun = new ServiceBase[] { new HealthMonitoringService(webAppPath) };
                            ServiceBase.Run(ServicesToRun);
                        }
                        else
                        {
                            // Create or delete counters
                            if (createCounters || deleteCounters)
                            {
                                // Set web application physical path
                                SystemContext.WebApplicationPhysicalPath = webAppPath;

                                // Use web application settings
                                SystemContext.UseWebApplicationConfiguration = true;

                                CMSApplication.Init();

                                // Register OnProgressLog event
                                HealthMonitoringManager.OnProgressLog += HealthMonitoringManager_OnProgressLog;

                                if (createCounters)
                                {
                                    showUsage = false;
                                    // Create general and sites categories
                                    HealthMonitoringManager.CreateCounterCategories();
                                    // Write message
                                    Console.WriteLine("Performance counters for the specified instance of Kentico were registered successfully.");
                                }
                                else if (deleteCounters)
                                {
                                    showUsage = false;
                                    // Delete general and sites categories
                                    HealthMonitoringManager.DeleteCounterCategories();
                                    // Write message
                                    Console.WriteLine("Performance counters for the specified instance of Kentico were removed successfully.");
                                }
                            }
                        }
                    }
                }

                // Show help
                if (showUsage)
                {
                    Console.WriteLine(USAGE);
                }
            }
            catch (Exception ex)
            {
                ServiceHelper.LogException(WinServiceHelper.HM_SERVICE_BASENAME, ex, true);
            }
        }


        /// <summary>
        /// Handles OnProgressLog event.
        /// </summary>
        /// <param name="message">Message</param>
        private static void HealthMonitoringManager_OnProgressLog(string message)
        {
            Console.WriteLine(message);
        }

        #endregion
    }
}