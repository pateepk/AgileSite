namespace CMS.Ecommerce
{
    /// <summary>
    /// PayPal payment result.
    /// </summary>
    public class PayPalPaymentResultInfo : PaymentResultInfo
    {
        private const string HEADER_DETAIL_URL = "{$PaymentGateway.PayPalResult.Detail.Url$}";
        private const string HEADER_PAYMENT_ID = "{$PaymentGateway.Result.PaymentId$}";

        private const string DETAIL_URL = "detailUrl";
        private const string PAYMENT_ID = "paymentId";


        /// <summary>
        /// Gets or sets the url with payment detail.
        /// </summary>
        public string PaymentDetailUrl
        {
            get
            {
                var item = EnsurePaymentResultItemInfo(DETAIL_URL, HEADER_DETAIL_URL);
                return item.Value;
            }
            set
            {
                var item = EnsurePaymentResultItemInfo(DETAIL_URL, HEADER_DETAIL_URL);
                item.Value = value;

                SetPaymentResultItemInfo(item);
            }
        }


        /// <summary>
        /// Gets or sets the payment ID.
        /// </summary>
        public string PaymentId
        {
            get
            {
                var item = EnsurePaymentResultItemInfo(PAYMENT_ID, HEADER_PAYMENT_ID);
                return item.Value;
            }
            set
            {
                var item = EnsurePaymentResultItemInfo(PAYMENT_ID, HEADER_PAYMENT_ID);
                item.Value = value;

                SetPaymentResultItemInfo(item);
            }
        }
    }
}