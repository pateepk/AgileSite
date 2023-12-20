using CMS.Core;

namespace CMS.Newsletters
{
    /// <summary>
    /// Web farm task used to clear newsletter license limitation tables.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    internal class ClearLicenseLimitationCacheWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="INewsletterLicenseCheckerService.ClearLicNewsletter"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            Service.Resolve<INewsletterLicenseCheckerService>().ClearLicNewsletter(false);
        }
    }
}
