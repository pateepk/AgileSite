namespace Kentico.OnlineMarketing.Web.Mvc
{
    internal static class ABTestRouteConstants
    {
        /// <summary>
        /// Name of the controller for AB test conversion logger.
        /// </summary>
        public const string ABTEST_CONVERSION_LOGGER_CONTROLLER_NAME = "KenticoABTestLogger";


        /// <summary>
        /// Name of the Page visit conversion logger route.
        /// </summary>
        public const string PAGE_VISIT_CONVERSION_LOGGER_ROUTE_NAME = "KenticoPageVisitConversionLogger";


        /// <summary>
        /// Name of the A/B test logger route.
        /// </summary>
        public const string ABTEST_LOGGER_SCRIPT_ROUTE_NAME = "KenticoABTestConversionLoggerScript";


        /// <summary>
        /// Route for Page visit conversion logger action.
        /// </summary>
        public const string PAGE_VISIT_CONVERSION_LOGGER_ROUTE = "Kentico.ABTest/PageVisitConversionLogger/Log";


        /// <summary>
        /// Route data key used to provide culture identifier.
        /// </summary>
        public const string CULTURE_ROUTE_DATA_KEY = "Kentico.ABTest.Culture";


        /// <summary>
        /// Route data key used to provide A/B test name identifier.
        /// </summary>
        public const string ABTEST_ROUTE_DATA_KEY = "abtest";


        /// <summary>
        /// Route for A/B test conversion logger script retrieval action.
        /// </summary>
        public const string ABTEST_LOGGER_SCRIPT_ROUTE = "Kentico.Resource/ABTest/KenticoABTestLogger/{" + CULTURE_ROUTE_DATA_KEY + "}/ConversionLogger.js";
    }
}
