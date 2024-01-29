using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;

using CMS.Base;
using CMS.Core;
using CMS.ModuleUsageTracking;

[assembly: RegisterModuleUsageDataSource(typeof(GlobalUsageDataSource))]

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Class for retrieving global statistical information about current web application instance.
    /// </summary>
    internal class GlobalUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Name of the global data source.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Global";
            }
        }


        /// <summary>
        /// Get the global statistical information about current web application instance.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var result = ObjectFactory<IModuleUsageDataCollection>.New();
            
            // Get current runtime version
            result.Add("ClrVersion", Environment.Version.ToString());

            // Get .NET version of this application
            var section = ConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            result.Add("DotNetVersion", (section == null) ? null : section.TargetFramework);
            
            // Get current OS information
            result.Add("OsVersion", Environment.OSVersion.ToString());
            result.Add("OsIs64Bit", Environment.Is64BitOperatingSystem);

            return result;
        }
    }
}
