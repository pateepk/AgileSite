namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Holds constants related to form component functionality.
    /// </summary>
    internal static class FormComponentConstants
    {
        /// <summary>
        /// Can be used for <see cref="RegisterFormComponentAttribute.ViewName"/> property.If used, view name path is automatically resolved to system default prefix.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This API supports the framework infrastructure and is not intended to be used directly from your code.
        /// </para>
        /// <para>
        /// Registration with identifier "company.somecomponent" will be registered as "~/Views/Shared/Kentico/FormComponents/somecomponent.cshtml" 
        /// </para>
        /// </remarks>
        public const string AutomaticSystemViewName = "__kenticosystemviewname";
    }
}
