namespace PaymentProcessor
{
    /// <summary>
    /// Base for The Dark pool recurring payment.
    /// </summary>
    public class SWRecurringPaymentPage : RecurringPaymentPageBase
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
                return "SW";
            }
        }

        #endregion settings

        #endregion properties
    }
}