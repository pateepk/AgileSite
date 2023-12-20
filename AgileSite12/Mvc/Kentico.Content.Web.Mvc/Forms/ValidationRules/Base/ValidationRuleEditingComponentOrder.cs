namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains constants describing order of editable properties within <see cref="ValidationRule"/>.
    /// </summary>
    public static class ValidationRuleEditingComponentOrder
    {
        /// <summary>
        /// Order of error message editing component. Returns <c>100</c>.
        /// </summary>
        public const int ERROR_MESSAGE = 100;


        /// <summary>
        /// Order of instance identifier editing component. Returns <c>1000</c>.
        /// </summary>
        public const int INSTANCE_IDENTIFIER = 1000;
    }
}
