using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;

using CMS.AzureCloudServices;

using Microsoft.WindowsAzure.ServiceRuntime;

[assembly: PreApplicationStartMethod(typeof(RoleEnvironmentHandlers), "ApplicationInit")]

namespace CMS.AzureCloudServices
{
    /// <summary>
    /// Handlers for <see cref="RoleEnvironment"/> events used for application validity.
    /// </summary>
    public sealed class RoleEnvironmentHandlers
    {
        /// <summary>
        /// Initializes handlers required by application.
        /// </summary>
        /// <remarks>
        /// This method is called automatically and should not be used in custom code.
        /// </remarks>
        public static void ApplicationInit()
        {
            if (!RoleEnvironment.IsAvailable)
            {
                return;
            }

            RoleEnvironment.Changing += RestartInstanceOnConfigurationChange;
            RoleEnvironment.Stopping += TryWaitForPendingRequests;
        }


        /// <summary>
        /// Role environment changing event. Fired before change is applied.
        /// </summary>
        private static void RestartInstanceOnConfigurationChange(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }


        private static void TryWaitForPendingRequests(object sender, RoleEnvironmentStoppingEventArgs e)
        {
            try
            {
                Trace.TraceInformation("[CMSApp.WebRole.OnStop]: OnStop called");
                var performanceCounter = new PerformanceCounter("ASP.NET", "Requests Current", "");

                while (true)
                {
                    var requestsCount = performanceCounter.NextValue();
                    Trace.TraceInformation("[CMSApp.WebRole.OnStop]: Pending requests count: " + requestsCount);
                    if (requestsCount <= 0)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}
