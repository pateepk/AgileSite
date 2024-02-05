
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Authorize.NET payment result.
    /// </summary>
    public class AuthorizeNetPaymentResultInfo : PaymentResultInfo
    {
        private const string HEADER_AUTHORIZATIONCODE = "{$authorizenet.authorizationcode$}";

        /// <summary>
        /// Authorization code.
        /// </summary>
        public string AuthorizationCode
        {
            get
            {
                var itemObj = EnsurePaymentResultItemInfo("authorizationcode", HEADER_AUTHORIZATIONCODE);

                return (itemObj?.Value) ?? "";
            }
            set
            {
                var itemObj = EnsurePaymentResultItemInfo("authorizationcode", HEADER_AUTHORIZATIONCODE);
                if (itemObj != null)
                {
                    itemObj.Value = ValidationHelper.GetString(value, "");
                    SetPaymentResultItemInfo(itemObj);
                }
            }
        }


        /// <summary>
        /// Creates base payment result info object + add Authorize.NET payment result properties.
        /// </summary>
        public AuthorizeNetPaymentResultInfo()
        {
            // Authorization code
            EnsurePaymentResultItemInfo("authorizationcode", HEADER_AUTHORIZATIONCODE);
        }
    }
}