using System.Collections.Generic;

using CMS.Base.Web.UI;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Form which is used as a base class for other payment gateways' forms.
    /// </summary>
    public abstract class CMSPaymentGatewayForm : AbstractUserControl
    {
        private CMSPaymentGatewayProvider mPaymentProvider;


        /// <summary>
        /// Gets the payment provider which initialized this control.
        /// </summary>
        /// <remarks>
        /// Any data transferred from or to this form needs to be done using <see cref="CMSPaymentGatewayProvider"/> implementation.
        /// </remarks>
        public CMSPaymentGatewayProvider PaymentProvider
        {
            get
            {
                return mPaymentProvider;
            }
            internal set
            {
                mPaymentProvider = value;
                OnInitialized();
            }
        }


        /// <summary>
        /// Indicates form initialization, property <see cref="PaymentProvider"/> is already set.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }


        /// <summary>
        /// Validates form data and returns error message if some error occurs.
        /// </summary>
        public virtual string ValidateData()
        {
            var data = GetPaymentGatewayData();
            return PaymentProvider?.ValidateCustomData(data);
        }


        /// <summary>
        /// Returns payment related data stored in form.
        /// </summary>
        public virtual IDictionary<string, object> GetPaymentGatewayData()
        {
            return new Dictionary<string, object>();
        }
    }
}