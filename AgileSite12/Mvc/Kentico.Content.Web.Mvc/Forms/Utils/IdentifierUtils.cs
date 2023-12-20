using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Utility methods for form builder object identifiers.
    /// </summary>
    internal static class IdentifierUtils
    {
        /// <summary>
        /// Returns valid identifier from given <paramref name="input"/> .
        /// </summary>
        public static string GetIdentifier(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException(nameof(input));
            }

            return input.Substring(input.LastIndexOf(".", StringComparison.Ordinal) + 1);
        }
    }
}
