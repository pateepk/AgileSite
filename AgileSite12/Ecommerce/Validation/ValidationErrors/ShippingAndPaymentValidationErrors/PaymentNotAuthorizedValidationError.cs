using CMS.Base;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents a validation error resulting from user not being authorized for payment.
    /// </summary>
    /// <seealso cref="CreateOrderValidator"/>
    public class PaymentNotAuthorizedValidationError : IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        public string MessageKey => "com.payment.notauthorized";


        /// <summary>
        /// Error message resulting from payment authorization.
        /// </summary>
        public string ErrorMessage { get; }


        /// <summary>
        /// Returns array containing error message.
        /// </summary>
        public object[] MessageParameters => new string[] { ErrorMessage };


        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentNotAuthorizedValidationError"/> class.
        /// </summary>
        public PaymentNotAuthorizedValidationError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
