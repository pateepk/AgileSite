using System;
using System.Linq;

using CMS.DocumentEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.DocumentEngine.PageBuilder.Internal;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Loads Page builder configuration either from a page or temporary data.
    /// </summary>
    internal class PageBuilderConfigurationSourceLoader : IPageBuilderConfigurationSourceLoader
    {
        private const string TEMP_GUID_FIELD_NAME = "PageBuilderWidgetsGUID";

        private readonly Guid editingInstanceIdentifier;
        private readonly IPageBuilderConfigurationSourceLoader loader;


        /// <summary>
        /// Creates an instance of <see cref="PageBuilderConfigurationSourceLoader"/> class.
        /// </summary>
        /// <param name="editingInstanceIdentifier">The editing instance identifier.</param>
        /// <param name="loader">The native loader to be used to retrieve the data.</param>
        public PageBuilderConfigurationSourceLoader(Guid editingInstanceIdentifier, IPageBuilderConfigurationSourceLoader loader)
        {
            this.editingInstanceIdentifier = editingInstanceIdentifier;
            this.loader = loader;
        }


        /// <summary>
        /// Loads Page builder configuration from temporary data or the provided page if not found in temporary data.
        /// </summary>
        /// <param name="page">Page from which the configuration will be loaded.</param>
        /// <returns>Returns the configuration source.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public PageBuilderConfigurationSource Load(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var tempData = GetTempData();
            if (tempData == null)
            {
                return loader.Load(page);
            }

            return new PageBuilderConfigurationSource
            {
                WidgetsConfiguration = tempData.PageBuilderWidgetsConfiguration,
                PageTemplateConfiguration = tempData.PageBuilderTemplateConfiguration
            };
        }


        private TempPageBuilderWidgetsInfo GetTempData()
        {
            if (editingInstanceIdentifier == Guid.Empty)
            {
                return null;
            }

            return TempPageBuilderWidgetsInfoProvider.GetPageBuilderWidgets()
                .WhereEquals(TEMP_GUID_FIELD_NAME, editingInstanceIdentifier)
                .TopN(1)
                .FirstOrDefault();
        }
    }
}
