using System.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Page for render report's subscriptions without HTTP context
    /// </summary>
    internal class ReportSubscriptionPage : Page
    {
        /// <summary>
        /// Enables render outside form tag
        /// </summary>
        /// <param name="control">Control</param>
        public override void VerifyRenderingInServerForm(Control control)
        {
            return;
        }
    }
}
