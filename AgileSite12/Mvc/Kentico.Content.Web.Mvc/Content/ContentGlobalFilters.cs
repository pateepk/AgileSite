using System;
using System.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Class that provides registration of all the content-related global filters.
    /// </summary>
    internal static class ContentGlobalFilters
    {
        /// <summary>
        /// Registers content-related global filters.
        /// </summary>
        public static void Register()
        {
            var globalFilters = GlobalFilters.Filters;

            globalFilters.Add(new ContentOutputActionFilter());
        }
    }


    /// <summary>
    /// Class that provides registration of all the required global filters for the page preview feature.
    /// </summary>
    [Obsolete("Preview global filters are registered automatically.")]
    public static class PreviewGlobalFilters
    {
        /// <summary>
        /// Registers global filters for the Preview feature.
        /// </summary>
        public static void Register()
        {
            
        }
    }
}
