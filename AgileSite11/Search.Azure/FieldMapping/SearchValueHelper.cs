namespace CMS.Search.Azure
{
    internal static class SearchValueHelper
    {
        /// <summary>
        /// Escapes characters for usage in filter expression.
        /// </summary>
        /// <param name="text">Text to escape characters in.</param>
        /// <returns>Escaped text for usage in filter expression.</returns>
        public static string EscapeSearchFilterValue(string text)
        {
            return text.Replace("'", "''");
        }
    }
}
