namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Container object for additional data used upon checking "create" page permission check.
    /// </summary>
    internal class CreatePagePermissionCheckAdditionalData
    {
        /// <summary>
        /// Culture.
        /// </summary>
        public string Culture { get; set; }


        /// <summary>
        /// Page type.
        /// </summary>
        public string PageType { get; set; }
    }
}