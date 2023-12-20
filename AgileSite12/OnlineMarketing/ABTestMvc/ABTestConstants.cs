namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Defines constants for A/B test module.
    /// </summary>
    public static class ABTestConstants
    {
        /// <summary>
        /// Defines a query string parameter that indicates which A/B variant is to be displayed in the UI.
        /// </summary>
        public const string AB_TEST_VARIANT_QUERY_STRING_PARAMETER_NAME = "displayabvariant";


        /// <summary>
        /// Code name of the first session conversion.
        /// </summary>
        public const string ABSESSIONCONVERSION_FIRST = "absessionconversionfirst";


        /// <summary>
        /// Code name of the recurring session conversion.
        /// </summary>
        internal const string ABSESSIONCONVERSION_RECURRING = "absessionconversionrecurring";


        /// <summary>
        /// Code name of the conversion.
        /// </summary>
        internal const string ABCONVERSION = "abconversion";


        /// <summary>
        /// Prefix of A/B test cookie names.
        /// </summary>
        internal const string AB_COOKIE_PREFIX = "CMSAB";
    }
}