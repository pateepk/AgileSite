using System.Diagnostics;
using System.Reflection;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Provides a check of MVC and administration applications hotfix versions.
    /// </summary>
    internal class HotfixVersionChecker
    {
        private const string HOTFIX_VERSION_KEY_NAME = "CMSHotfixVersion";
        private IEventLogService eventLogService;
        private bool developmentMode;


        /// <summary>
        /// Initializes a new instance of the <see cref="HotfixVersionChecker"/> class.
        /// </summary>
        public HotfixVersionChecker() : this(SystemContext.DevelopmentMode, Service.Resolve<IEventLogService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HotfixVersionChecker"/> class.
        /// </summary>
        /// <param name="developmentMode">Indicates whether the application runs in a development mode.</param>
        /// <param name="eventLogService">The event log service.</param>
        internal HotfixVersionChecker(bool developmentMode, IEventLogService eventLogService)
        {
            this.eventLogService = eventLogService;
            this.developmentMode = developmentMode;
        }


        /// <summary>
        /// Checks the hotfix versions of MVC and administration applications.
        /// </summary>
        public void CheckHotfixVersion()
        {
            if (developmentMode)
            {
                // Do not check hotfix versions in SLN. Assembly version (stored in the GlobaAssemblyInfo.cs) is always x.0.0 in SLN
                // however hotfix version in the Settings table can vary.
                return;
            }

            string mainApplicationHotfixVersion = GetMainApplicationHotfixVersion();
            string mvcApplicationHotfixVersion = GetMvcApplicationHotfixVersion();

            if (!mainApplicationHotfixVersion.Equals(mvcApplicationHotfixVersion, System.StringComparison.OrdinalIgnoreCase))
            {
                eventLogService.LogEvent(EventType.WARNING, "HotfixVersionChecker", "DifferentHotfixVersions", $"The hotfix version of the MVC application ({mvcApplicationHotfixVersion}) and the administration application ({mainApplicationHotfixVersion}) do not match. Update the version of Kentico NuGet packages in your MVC application to match the hotfix version of the administration application.");
            }
        }


        internal virtual string GetMvcApplicationHotfixVersion()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var assemblyFileInfo = FileVersionInfo.GetVersionInfo(thisAssembly.Location);

            return assemblyFileInfo.ProductVersion;
        }


        internal virtual string GetMainApplicationHotfixVersion()
        {
            var hotfixVersion = SettingsKeyInfoProvider.GetIntValue(HOTFIX_VERSION_KEY_NAME);

            return $"{DatabaseHelper.DatabaseVersion}.{hotfixVersion}";
        }
    }
}
