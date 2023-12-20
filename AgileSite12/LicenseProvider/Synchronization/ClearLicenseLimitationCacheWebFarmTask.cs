using CMS.Core;

namespace CMS.LicenseProvider
{
    /// <summary>
    /// Web farm task used to clear basic license limitation tables.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    internal class ClearLicenseLimitationCacheWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="LicenseHelper.ClearLicenseLimitation"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            LicenseHelper.ClearLicenseLimitation(false);
        }
    }
}
