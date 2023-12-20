using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Win services helper class.
    /// </summary>
    public static class WinServiceHelper
    {
        #region "Constants"

        /// <summary>
        /// Extesion of the watcher file.
        /// </summary>
        public const string WATCHER_FILE_EXTENSION = ".srvc";

        /// <summary>
        /// Services definition file.
        /// </summary>
        public const string SERVICES_FILE = "services.xml";

        /// <summary>
        /// Prefix for services.
        /// </summary>
        public const string SERVICE_NAME_PREFIX = "KenticoCMS";

        #endregion


        #region "Services constants"

        /// <summary>
        /// Scheduler service base name.
        /// </summary>
        public const string SCHEDULER_SERVICE_BASENAME = SERVICE_NAME_PREFIX + "Scheduler";


        /// <summary>
        /// Health Monitoring service base name.
        /// </summary>
        public const string HM_SERVICE_BASENAME = SERVICE_NAME_PREFIX + "HealthMonitor";

        #endregion


        #region "Variables"

        private static string mServicesDataPath = null;
        private static string mServicesFilePath = null;
        private static List<WinServiceItem> mServicesDefinition = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Physical path to the win services data folder
        /// </summary>
        public static string ServicesDataPath
        {
            get
            {
                return mServicesDataPath ?? (mServicesDataPath = string.Format("{0}\\App_Data\\CMSModules\\WinServices\\", SystemContext.WebApplicationPhysicalPath));
            }
            set
            {
                mServicesDataPath = value;
            }
        }


        /// <summary>
        /// Physical path to the file that contains available services.
        /// </summary>
        public static string ServicesFilePath
        {
            get
            {
                return mServicesFilePath ?? (mServicesFilePath = ServicesDataPath.TrimEnd('\\') + "\\" + SERVICES_FILE);
            }
        }


        /// <summary>
        /// List of services definition.
        /// </summary>
        public static List<WinServiceItem> ServicesDefinition
        {
            get
            {
                if (mServicesDefinition == null)
                {
                    mServicesDefinition = new List<WinServiceItem>();

                    if (File.Exists(ServicesFilePath))
                    {
                        XmlDocument document = new XmlDocument();
                        document.Load(ServicesFilePath);

                        // Select node 'Services'
                        XmlNodeList nodes = document.SelectNodes("//Service");

                        if ((nodes != null) && (nodes.Count > 0))
                        {
                            foreach (XmlNode node in nodes)
                            {
                                // Create service item
                                WinServiceItem service = new WinServiceItem(node);
                                // Add service item to list
                                mServicesDefinition.Add(service);
                            }
                        }
                    }
                }

                return mServicesDefinition;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets file name of the watcher file for given windows service.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static string GetServiceWatcherFileName(string serviceName)
        {
            // Get safe file name
            string fileName = null;
            if (!string.IsNullOrEmpty(serviceName))
            {
                fileName = ValidationHelper.GetSafeFileName(serviceName, null) + WATCHER_FILE_EXTENSION;
            }

            return fileName;
        }


        /// <summary>
        /// Gets path to the watcher file for given windows service.
        /// </summary>
        /// <param name="serviceName">Service name (If no service name given, service file watcher folder is returned.)</param>
        public static string GetServiceWatcherFilePath(string serviceName)
        {
            return ServicesDataPath + GetServiceWatcherFileName(serviceName);
        }


        /// <summary>
        /// Restarts given windows service(s).
        /// </summary>
        /// <param name="serviceDefinitionName">Service definition name (If no service name given, all services are restarted.)</param>
        /// <param name="logTask">Indicates whether web farm task should be logged.</param>
        /// <seealso cref="HM_SERVICE_BASENAME"/>
        /// <seealso cref="SCHEDULER_SERVICE_BASENAME"/>
        public static void RestartService(string serviceDefinitionName, bool logTask)
        {
            var serviceName = GetServiceDefinition(serviceDefinitionName)?.GetServiceName();
            if (serviceDefinitionName == null || serviceName != null)
            {
                RestartService(serviceName);

                if (logTask)
                {
                    WebFarmHelper.CreateTask(new RestartServiceWebFarmTask { ServiceDefinitionName = serviceDefinitionName });
                }
            }
        }


        /// <summary>
        /// Restarts given windows service(s).
        /// </summary>
        /// <param name="serviceName">Service name (If no service name given, all services are restarted.)</param>
        private static void RestartService(string serviceName)
        {
            if (Directory.Exists(ServicesDataPath))
            {
                // Get watcher path
                string path = GetServiceWatcherFilePath(serviceName);
                string timeStamp = DateTime.Now.ToString();

                // Service folder, restart all services
                if (serviceName == null)
                {
                    string[] files = Directory.GetFiles(path, "*" + WATCHER_FILE_EXTENSION);
                    foreach (string file in files)
                    {
                        // Touch file to restart service
                        File.WriteAllText(file, timeStamp);
                    }
                }
                else
                {
                    // Touch file to restart service
                    if (File.Exists(path))
                    {
                        File.WriteAllText(path, timeStamp);
                    }
                }
            }
        }


        /// <summary>
        /// Deletes service restart file.
        /// </summary>
        /// <param name="serviceDefinitionName">Service definition name (If no service name given, all services are restarted.)</param>
        /// <param name="logTask">Indicates whether web farm task should be logged.</param>
        /// <seealso cref="HM_SERVICE_BASENAME"/>
        /// <seealso cref="SCHEDULER_SERVICE_BASENAME"/>
        public static void DeleteServiceFile(string serviceDefinitionName, bool logTask)
        {
            var serviceName = GetServiceDefinition(serviceDefinitionName)?.GetServiceName();
            if (serviceDefinitionName == null || serviceName != null)
            {
                DeleteServiceFile(serviceName);

                if (logTask)
                {
                    WebFarmHelper.CreateTask(new DeleteServiceWebFarmTask { ServiceDefinitionName = serviceDefinitionName });
                }
            }
        }

        /// <summary>
        /// Deletes service restart file.
        /// </summary>
        /// <param name="serviceName">Service name (If no service name given, all services are restarted.)</param>
        private static void DeleteServiceFile(string serviceName)
        {
            if (Directory.Exists(ServicesDataPath))
            {
                // Get watcher path
                string path = GetServiceWatcherFilePath(serviceName);

                // Service folder, restart all services
                if (serviceName == null)
                {
                    string[] files = Directory.GetFiles(path, "*" + WATCHER_FILE_EXTENSION);
                    foreach (string file in files)
                    {
                        // Delete file
                        File.Delete(file);
                    }
                }
                else
                {
                    // Delete file
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }


        /// <summary>
        /// Indicates if there is at least one service watcher file available
        /// </summary>
        public static bool ServicesAvailable()
        {
            string[] files = Directory.GetFiles(ServicesDataPath, "*" + WATCHER_FILE_EXTENSION);
            return (files.Length > 0);
        }


        /// <summary>
        /// Clears services definition.
        /// </summary>
        public static void ClearServicesDefinition()
        {
            mServicesFilePath = null;
            mServicesDataPath = null;
            mServicesDefinition = null;
        }


        /// <summary>
        /// Formats the Windows service name using the specified values.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <param name="applicationName">The application name.</param>
        /// <param name="applicationIdentifier">The application identifier.</param>
        /// <returns>The Windows service name.</returns>
        public static string FormatServiceName(string format, string applicationName, string applicationIdentifier)
        {
            // ## Special case - backward compatibility for older versions. Substitute placeholders with application settings values.
            // In old services.xml, the placeholder was {0} and expected GUID which was not user-friendly, but this has to be preserved
            // in order to be able to uninstall old services with new versions of KIM. New versions of services.xml use {1} placeholder (no {0} placeholder)
            // and expect application code name there.
            string name = format.Replace("{0}", applicationIdentifier).Replace("{1}", String.Format("({0})", applicationName));

            // Return service name without unsafe characters
            return TextHelper.LimitLength(name.Replace('\\', '_').Replace('/', '_'), 80, "", false);
        }


        /// <summary>
        /// Formats the Windows service display name using the specified values.
        /// </summary>
        /// <param name="format">The string to format.</param>
        /// <param name="applicationName">The application name.</param>
        /// <returns>The Windows service display name.</returns>
        public static string FormatServiceDisplayName(string format, string applicationName)
        {
            return String.Format(format, applicationName);
        }

        #endregion


        #region "Service definition methods"

        /// <summary>
        /// Gets sevice definition item by specific base name.
        /// </summary>
        /// <param name="baseName">Base name of service</param>
        public static WinServiceItem GetServiceDefinition(string baseName)
        {
            if (!string.IsNullOrEmpty(baseName))
            {
                return ServicesDefinition.Find(s => CMSString.Compare(s.BaseName, baseName, true) == 0);
            }

            return null;
        }

        #endregion
    }
}