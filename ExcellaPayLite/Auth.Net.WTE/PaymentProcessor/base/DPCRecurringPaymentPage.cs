using PaymentProcessor.net.authorize.apitest;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using WTE.Configuration;
using WTE.Helpers;

namespace PaymentProcessor
{
    /// <summary>
    /// Base for The Dark pool recurring payment.
    /// </summary>
    public class DPCRecurringPaymentPage : RecurringPaymentPageBase
    {
        #region properties

        #region settings

        /// <summary>
        /// Setting prefix
        /// </summary>
        protected override string SiteSettingPrefix
        {
            get
            {
                return "DPC";
            }
        }

        #endregion

        #endregion
    }
}