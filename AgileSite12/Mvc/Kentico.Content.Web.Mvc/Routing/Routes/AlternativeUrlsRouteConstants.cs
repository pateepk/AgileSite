namespace Kentico.Content.Web.Mvc.Routing
{
    internal static class AlternativeUrlsRouteConstants
    {
        /// <summary>
        /// Catch-all route for Alternative URLs feature.
        /// </summary>
        public const string CATCH_ALL_ROUTE = "{*" + CATCH_ALL_ROUTE_DATA_KEY + "}";


        /// <summary>
        /// Route data key used to provide the URL.
        /// </summary>
        public const string CATCH_ALL_ROUTE_DATA_KEY = "Kentico.AlternativeUrls.Url";


        /// <summary>
        /// Name of catch-all route for Alternative URLs feature.
        /// </summary>
        public const string CATCH_ALL_ROUTE_NAME = "Kentico.AlternativeUrls.CatchAll";


        /// <summary>
        /// Name of context item that provides URL for redirection.
        /// </summary>
        public const string REDIRECT_URL_CONTEXT_ITEM_NAME = "Kentico.AlternativeUrls.RedirectUrl";


        /// <summary>
        /// Name of context item that provides page main URL after rewrite.
        /// </summary>
        public const string PAGE_MAIN_URL_CONTEXT_ITEM_NAME = "Kentico.AlternativeUrls.PageMainUrl";
    }
}
