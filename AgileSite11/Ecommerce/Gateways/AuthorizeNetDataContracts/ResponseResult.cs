
namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Envelope for representing Authorize.NET response, which could be of type <see cref="ErrorResponse"/> or <see cref="TransactionRespone"/>.
    /// </summary>
    public class ResponseResult
    {
        /// <summary>
        /// Transaction response.
        /// </summary>
        public CreateTransactionResponse TransactionRespone
        {
            get;
            set;
        }


        /// <summary>
        /// Error response.
        /// </summary>
        public ErrorResponse ErrorResponse
        {
            get;
            set;
        }
    }
}
