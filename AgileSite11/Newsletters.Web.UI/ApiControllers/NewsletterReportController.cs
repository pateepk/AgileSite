using System.Collections.Generic;
using System.Web.Http;

using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters.Web.UI.Internal;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(NewsletterReportController))]

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Provides data for newsletter report.
    /// </summary>
    [IsFeatureAvailable(FeatureEnum.Newsletters)]
    [AllowOnlyEditor]
    [IsAuthorizedPerResource(ModuleName.NEWSLETTER, "Read")]
    [HandleExceptions]
    public sealed class NewsletterReportController : CMSApiController
    {
        private readonly INewsletterReportDataProvider mReportDataProvider;


        /// <summary>
        /// Constructor. Inner instance of <see cref="INewsletterReportDataProvider"/> is obtained from the Kentico object factory.
        /// </summary>
        public NewsletterReportController()
            : this(Service.Resolve<INewsletterReportDataProvider>())
        {
        }


        internal NewsletterReportController(INewsletterReportDataProvider reportDataProvider)
        {
            mReportDataProvider = reportDataProvider;
        }


        /// <summary>
        /// Returns a collection of email statistics.
        /// </summary>
        /// <param name="newsletterId">ID of the email feed where the email belong.</param>
        [HttpGet]
        public IEnumerable<NewsletterEmailsDataViewModel> GetEmailsData(int newsletterId)
        {
            return mReportDataProvider.GetEmailsData(newsletterId);
        }
    }
}
