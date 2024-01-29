using System.Runtime.Serialization;

using CMS.Helpers;

namespace CMS.Ecommerce.AuthorizeNetDataContracts
{
    /// <summary>
    /// Authorize.NET API - Type of transaction response.
    /// </summary>
    [DataContract(Namespace = AuthorizeNetParameters.API_NAMESPACE)]
    public enum TransactionResponseCode
    {
        /// <summary>
        /// Transaction has been approved.
        /// </summary>
        [EnumStringRepresentation("approved")]
        [EnumMember(Value = "1")]
        Approved = 1,

        /// <summary>
        /// Transaction has been declined.
        /// </summary>
        [EnumMember(Value = "2")]
        [EnumStringRepresentation("declined ")]
        Declined = 2,

        /// <summary>
        /// An error occured during transaction processing.
        /// </summary>
        [EnumMember(Value = "3")]
        [EnumStringRepresentation("error")]
        Error = 3,

        /// <summary>
        /// Transaction has been held for review.
        /// </summary>
        [EnumMember(Value = "4")]
        [EnumStringRepresentation("heldforreview")]
        HeldForReview = 4
    }
}
