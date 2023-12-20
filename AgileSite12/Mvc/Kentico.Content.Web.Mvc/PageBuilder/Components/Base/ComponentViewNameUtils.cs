namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Utility methods for component view name.
    /// </summary>
    internal static class ComponentViewNameUtils
    {
        /// <summary>
        /// Gets view name for component definition.
        /// </summary>
        /// <param name="identifier">Component definition identifier.</param>
        /// <param name="defaultFolderName">Default view folder name.</param>
        /// <param name="customViewName">Custom view name.</param>
        public static string GetViewName(string identifier, string defaultFolderName, string customViewName)
        {
            return string.IsNullOrEmpty(customViewName) ? $"{defaultFolderName}/_{identifier}" : customViewName;
        }
    }
}
