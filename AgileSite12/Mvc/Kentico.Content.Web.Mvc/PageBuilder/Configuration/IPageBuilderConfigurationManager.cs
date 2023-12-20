using System;

using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides interface used for managing the Page builder configuration.
    /// </summary>
    internal interface IPageBuilderConfigurationManager
    {
        /// <summary>
        /// Stores configuration to the storage.
        /// </summary>
        /// <param name="instanceGuid">Identifier of the editing instance.</param>
        /// <param name="configuration">Configuration to store.</param>
        void Store(Guid instanceGuid, string configuration);


        /// <summary>
        /// Changes configuration based on new page template and stores it to the storage.
        /// </summary>
        /// <param name="instanceGuid">Identifier of the editing instance.</param>
        /// <param name="templateIdentifier">Identifier of page template to set.</param>
        void ChangeTemplate(Guid instanceGuid, string templateIdentifier);


        /// <summary>
        /// Loads configuration from the storage.
        /// </summary>
        /// <param name="page">Page with the configuration.</param>
        PageBuilderConfiguration Load(TreeNode page);
    }
}
