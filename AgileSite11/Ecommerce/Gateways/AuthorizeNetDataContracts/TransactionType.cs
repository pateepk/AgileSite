using System.Runtime.Serialization;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Type of transaction.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public enum TransactionType
    {
        /// <summary>
        /// Authorize and capture transaction.
        /// </summary>
        [EnumMember(Value = "authCaptureTransaction")]
        AuthCaptureTransaction = 1,

        /// <summary>
        /// Authorize only transaction.
        /// </summary>
        [EnumMember(Value = "authOnlyTransaction")]
        AuthOnlyTransaction = 2,

        /// <summary>
        /// Capture previously authorized transaction.
        /// </summary>
        [EnumMember(Value = "priorAuthCaptureTransaction")]
        PriorAuthCaptureTransaction = 3
    }
}
