namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing names of Authorize.NET payment gateway required parameters.
    /// </summary>
    public static class AuthorizeNetParameters
    {
        /// <summary>
        /// Credit card CCV.
        /// </summary>
        public const string CARD_CCV = "cardccv";


        /// <summary>
        /// Credit card number.
        /// </summary>
        public const string CARD_NUMBER = "cardnumber";


        /// <summary>
        /// Credit card expiration.
        /// </summary>
        public const string CARD_EXPIRATION = "cardexpiration";


        /// <summary>
        /// Namespace of Authorize.NET API.
        /// </summary>
        public const string API_NAMESPACE = "AnetApi/xml/v1/schema/AnetApiSchema.xsd";
    }
}