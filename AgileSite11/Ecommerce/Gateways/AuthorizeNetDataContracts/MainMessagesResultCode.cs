using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Result code of payment transaction.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public enum MainMessagesResultCode
    {
        /// <summary>
        /// Payment transaction has been successfull.
        /// </summary>
        [EnumMember(Value = "Ok")]
        Ok = 1,

        /// <summary>
        /// Payment transaction ended with error.
        /// </summary>
        [EnumMember(Value = "Error")]
        Error = 2
    }
}
