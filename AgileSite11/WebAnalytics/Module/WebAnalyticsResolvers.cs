using CMS.MacroEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class WebAnalyticsResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mAnalyticsResolver = null;

        #endregion


        /// <summary>
        /// Returns web analytics resolver.
        /// </summary>
        public static MacroResolver AnalyticsResolver
        {
            get
            {
                if (mAnalyticsResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.PrioritizeProperty("CurrentUser");
                    resolver.PrioritizeProperty("ContactManagementContext");
                    resolver.PrioritizeProperty("CurrentDevice");

                    mAnalyticsResolver = resolver;
                }

                return mAnalyticsResolver;
            }
        }
    }
}