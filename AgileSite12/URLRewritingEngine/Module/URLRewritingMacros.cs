using CMS.Base;
using CMS.MacroEngine;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// Macro support for URL rewriting
    /// </summary>
    internal class URLRewritingMacros
    {
        /// <summary>
        /// Initializes the macros
        /// </summary>
        public static void Init()
        {
            MacroResolver globalResolver = MacroContext.GlobalResolver;

            globalResolver.SetNamedSourceDataCallback("RouteData", (x) => GetCurrentRouteData(), false);
            globalResolver.SetNamedSourceDataCallback("CurrentRouteData", (x) => GetCurrentRouteData(), false);
        }


        /// <summary>
        /// Gets the current route data as the macro object
        /// </summary>
        private static object GetCurrentRouteData()
        {
            var data = URLRewritingContext.CurrentRouteData;

            return (data != null ? new DictionaryContainer(data.Values) : null);
        }
    }
}
