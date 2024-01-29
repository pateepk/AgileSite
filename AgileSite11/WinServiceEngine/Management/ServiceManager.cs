using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using CMS.DataEngine;
using CMS.Base;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Windows services manager
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// Constant representing local machine string.
        /// </summary>
        public const string LOCAL_MACHINE_NAME = ".";

        /// <summary>
        /// Service timeout (seconds). Default is 30 seconds.
        /// </summary>
        public const int SERVICE_TIMEOUT_SECONDS = 30;


        /// <summary>
        /// Gets service with and service name for local machine.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static ServiceController GetService(string serviceName)
        {
            return GetService(LOCAL_MACHINE_NAME, serviceName);
        }


        /// <summary>
        /// Gets service with specified machine name and service name.
        /// </summary>
        /// <param name="machineName">Machine name</param>
        /// <param name="serviceName">Service name</param>
        public static ServiceController GetService(string machineName, string serviceName)
        {
            return ServiceController.GetServices(machineName).FirstOrDefault(s => CMSString.Compare(s.ServiceName, serviceName, true) == 0);
        }


        /// <summary>
        /// Gets services for local machine.
        /// </summary>
        public static List<ServiceController> GetServices()
        {
            return GetServices(LOCAL_MACHINE_NAME);
        }


        /// <summary>
        /// Gets services for specified machine.
        /// </summary>
        /// <param name="machineName">Machine name</param>
        public static List<ServiceController> GetServices(string machineName)
        {
            try
            {
                // Get services that start with string 'kenticocms'
                return ServiceController.GetServices(machineName).Where(sc => sc.ServiceName.StartsWithCSafe(WinServiceHelper.SERVICE_NAME_PREFIX, true)).ToList();
            }
            catch (Exception ex)
            {
                ServiceHelper.LogException(ServiceHelper.EVENT_LOG_NAME, ex, null, true);
            }

            return null;
        }


        /// <summary>
        /// Stops service.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static void StopService(string serviceName)
        {
            StopService(GetService(serviceName));
        }


        /// <summary>
        /// Stops service.
        /// </summary>
        /// <param name="service">Service controller</param>
        public static void StopService(ServiceController service)
        {
            if ((service != null) && (service.Status == ServiceControllerStatus.Running))
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, SERVICE_TIMEOUT_SECONDS));
            }
        }


        /// <summary>
        /// Starts service.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static void StartService(string serviceName)
        {
            StartService(GetService(serviceName));
        }


        /// <summary>
        /// Starts service.
        /// </summary>
        /// <param name="service">Service controller</param>
        public static void StartService(ServiceController service)
        {
            if ((service != null) && (service.Status == ServiceControllerStatus.Stopped))
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, SERVICE_TIMEOUT_SECONDS));
            }
        }


        /// <summary>
        /// Restarts service.
        /// </summary>
        /// <param name="serviceName">Service name</param>
        public static void RestartService(string serviceName)
        {
            // Get service
            ServiceController service = GetService(serviceName);
            // Stop service
            StopService(service);
            // Start service
            StartService(service);
        }


        /// <summary>
        /// Installs or uninstalls service.
        /// </summary>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="install">True, if install service. Otherwise uninstall service.</param>
        public static void InstallOrUninstall(string assemblyName, bool install)
        {
            string webAppPath = SystemContext.WebApplicationPhysicalPath;
            string assemblyPath = webAppPath + "\\bin\\" + assemblyName;

            if (File.Exists(assemblyPath))
            {
                // Prepare parameters
                string installParam = install ? "/i" : "/u";
                string webPathParam = "/" + ServiceHelper.WEB_PATH_PREFIX + webAppPath;
                string[] args = new string[] { webPathParam, installParam };

                // Create installer
                using (AssemblyInstaller installer = new AssemblyInstaller(assemblyPath, args))
                {
                    // Install service
                    if (install)
                    {
                        installer.Install(null);
                        installer.Commit(null);
                    }
                    // Uninstall service
                    else
                    {
                        installer.Uninstall(null);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("The '{0}' assembly does not exist.", assemblyPath));
            }
        }


        /// <summary>
        /// Uninstalls all available services.
        /// </summary>
        public static void UninstallServices()
        {
            foreach (WinServiceItem item in WinServiceHelper.ServicesDefinition)
            {
                // Check if service exists
                ServiceController service = GetService(item.GetServiceName());
                if (service != null)
                {
                    InstallOrUninstall(item.AssemblyName, false);
                }
            }
        }


        /// <summary>
        /// Installs all available services.
        /// </summary>
        /// <param name="startServices">Start services</param>
        public static void InstallServices(bool startServices)
        {
            InstallServices(WinServiceHelper.ServicesDefinition, startServices);
        }


        /// <summary>
        /// Installs given services.
        /// </summary>
        /// <param name="services">List of services to install</param>
        /// <param name="startServices">Start services</param>
        public static void InstallServices(List<WinServiceItem> services, bool startServices)
        {
            // Install services
            foreach (WinServiceItem item in services)
            {
                InstallOrUninstall(item.AssemblyName, true);
            }

            // Start services
            if (startServices)
            {
                foreach (WinServiceItem item in services)
                {
                    StartService(item.GetServiceName());
                }
            }
        }


        /// <summary>
        /// Indicates if at least one of the services is installed.
        /// </summary>
        public static bool ServicesInstalled()
        {
            return WinServiceHelper.ServicesDefinition.Select(item => GetService(item.GetServiceName())).Any(service => service != null);
        }
    }
}