using Microsoft.WindowsAzure.ServiceRuntime;

using CMS;
using CMS.AzureStorage;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.AzureCloudServices;

[assembly: RegisterModule(typeof(AzureCloudServicesModule))]

namespace CMS.AzureCloudServices
{
    /// <summary>
    /// Ensures consistency and validity of Instances running within Azure Cloud Service environment.
    /// </summary>
    internal class AzureCloudServicesModule : Module
    {
        public AzureCloudServicesModule()
            : base("CMS.AzureCloudServicesModule")
        {
        }


        protected override void OnPreInit()
        {
            base.OnPreInit();

            if (!RoleEnvironment.IsAvailable)
            {
                return;
            }

            // Setup Web farm server name (required for both install and application)
            ApplicationEvents.PreInitialized.Execute += (object sender, System.EventArgs e) =>
            {
                SystemContext.ServerName = ValidationHelper.GetCodeName(AzureHelper.CurrentInstanceID + "_" + AzureHelper.DeploymentID);
            };

            ApplicationStartInit();
       }


        /// <summary>
        /// Code executed on application start.
        /// </summary>
        private void ApplicationStartInit()
        {
            // Provide the configuration to the engine
            CMSAppSettings.GetApplicationSettings += GetApplicationSettings;
            CMSConnectionStrings.GetConnectionString += GetApplicationSettings;

            AzureHelper.CurrentInstanceID = RoleEnvironment.CurrentRoleInstance.Id;
            AzureHelper.OnRestartRequired += (sender, args) => RoleEnvironment.RequestRecycle();

            // Get path for Temp
            LocalResource temp = RoleEnvironment.GetLocalResource("AzureTemp");
            PathHelper.TempPath = temp.RootPath;

            // Get path for Cache
            LocalResource cache = RoleEnvironment.GetLocalResource("AzureCache");
            PathHelper.CachePath = cache.RootPath;

            // Get internal instance endpoints
            foreach (var instance in RoleEnvironment.Roles["CMSApp"].Instances)
            {
                // Current instance ID
                if (instance.Id == RoleEnvironment.CurrentRoleInstance.Id)
                {
                    // Set current internal endpoint
                    RoleInstanceEndpoint endpoint = instance.InstanceEndpoints["InternalHttpIn"];
                    AzureHelper.CurrentInternalEndpoint = "http://" + endpoint.IPEndpoint;
                }
            }

            // Set Azure deployment
            AzureHelper.DeploymentID = RoleEnvironment.DeploymentId;

            // Set number of instances
            AzureHelper.NumberOfInstances = RoleEnvironment.Roles["CMSApp"].Instances.Count;
        }


        /// <summary>
        /// Reads settings from service configuration file. 
        /// </summary>
        /// <param name="key">Setting key.</param>    
        private string GetApplicationSettings(string key)
        {
            try
            {
                return RoleEnvironment.GetConfigurationSettingValue(key);
            }
            catch
            {
                // Setting key was not found
                return null;
            }
        }
    }
}
