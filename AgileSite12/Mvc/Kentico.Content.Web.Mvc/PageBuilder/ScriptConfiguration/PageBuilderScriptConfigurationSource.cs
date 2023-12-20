using System.Threading;

using CMS.Base;
using CMS.Helpers;

using Kentico.Builder.Web.Mvc;
using Kentico.Components.Web.Mvc.Internal;
using Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Source of <see cref="PageBuilderScriptConfiguration"/>.
    /// </summary>
    public class PageBuilderScriptConfigurationSource
    {
        private static PageBuilderScriptConfigurationSource mInstance;


        /// <summary>
        /// An event raised upon getting <see cref="PageBuilderScriptConfiguration"/>. Allows for modification of <see cref="GetConfigurationEventArgs.Configuration"/> before
        /// being returned.
        /// </summary>
        public readonly GetConfigurationHandler GetConfiguration = new GetConfigurationHandler { Name = "PageBuilderScriptConfigurationSource.GetConfiguration" };


        /// <summary>
        /// Gets the <see cref="PageBuilderScriptConfigurationSource"/> instance.
        /// </summary>
        public static PageBuilderScriptConfigurationSource Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Interlocked.CompareExchange(ref mInstance, new PageBuilderScriptConfigurationSource(), null);
                }
                return mInstance;
            }
            internal set
            {
                mInstance = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PageBuilderScriptConfigurationSource"/> class.
        /// </summary>
        private PageBuilderScriptConfigurationSource()
        {
        }


        /// <summary>
        /// Gets <see cref="PageBuilderScriptConfiguration"/> based on provided <paramref name="pageBuilder"/>, <paramref name="applicationPath"/> and <paramref name="allowedDomainsRetriever"/>.
        /// </summary>
        /// <param name="pageBuilder">Page builder feature to retrieve page identifier from.</param>
        /// <param name="applicationPath">Application path for endpoints resolution.</param>
        /// <param name="allowedDomainsRetriever">Allowed domains retriever for allowed origins specification.</param>
        /// <returns></returns>
        internal PageBuilderScriptConfiguration Get(IPageBuilderFeature pageBuilder, string applicationPath, IAllowedDomainsRetriever allowedDomainsRetriever)
        {
            var configuration = new PageBuilderScriptConfiguration
            {
                PageIdentifier = pageBuilder.PageIdentifier,
                ApplicationPath = applicationPath,
                ConfigurationLoadEndpoint = $"{applicationPath?.TrimEnd('/')}/{PageBuilderRoutes.CONFIGURATION_LOAD_ROUTE.Replace("{pageId}", pageBuilder.PageIdentifier.ToString())}",
                ConfigurationStoreEndpoint = $"{applicationPath?.TrimEnd('/')}/{PageBuilderRoutes.CONFIGURATION_STORE_ROUTE}",
                MetadataEndpoint = $"{applicationPath?.TrimEnd('/')}/{PageBuilderRoutes.METADATA_ROUTE}",
                AllowedOrigins = allowedDomainsRetriever.Retrieve(),
                DevelopmentMode = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSDebugBuilderScripts"], false),
                PageTemplate = new PageTemplateScriptConfiguration(pageBuilder.GetDataContext().Page, applicationPath),
                Selectors = new SelectorsScriptConfiguration(applicationPath)
            };

            var eventArgs = new GetConfigurationEventArgs
            {
                Configuration = configuration
            };

            GetConfiguration.StartEvent(eventArgs);

            return eventArgs.Configuration;
        }
    }
}
