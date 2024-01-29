using System;

namespace CMS.DocumentEngine
{
    internal static class ExtensionMatcher
    {
        /// <summary>
        /// Returns true if current extension is correct extension for current set of extensions.
        /// </summary>
        /// <param name="extensionList">Extensions separated by semicolon.</param>
        /// <param name="extension">Extension to check if is present in <paramref name="extensionList"/>.</param>
        /// <remarks>When <paramref name="extensionList"/> is <c>null</c> or empty, method directly returns<c>false</c>.</remarks>
        public static bool IsExtensionInList(string extensionList, string extension)
        {
            if (String.IsNullOrEmpty(extensionList))
            {
                return false;
            }

            var extensionsSemicolons = ";" + extensionList + ";";
            return extensionsSemicolons.IndexOf(";" + extension + ";", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
