using System;

using CMS.DocumentEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Extension methods for <see cref="TreeNode"/>.
    /// </summary>
    internal static class TreeNodeExtensions
    {
        private const string DOCUMENT_PAGE_BUILDER_WIDGETS_COLUMN_NAME = "DocumentPageBuilderWidgets";
        private const string DOCUMENT_PAGE_TEMPLATE_CONFIGURATION_COLUMN_NAME = "DocumentPageTemplateConfiguration";
        private const string DOCUMENT_AB_TEST_CONFIGURATION = "DocumentABTestConfiguration";


        /// <summary>
        /// Gets page builder widgets configuration from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to retrieve configuration from.</param>
        /// <returns>Returns page builder widgets configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static string GetPageBuilderWidgets(this TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return page.GetStringValue(DOCUMENT_PAGE_BUILDER_WIDGETS_COLUMN_NAME, null);
        }


        /// <summary>
        /// Sets page builder widgets configuration to <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to set configuration to.</param>
        /// <param name="value">Configuration value to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static void SetPageBuilderWidgets(this TreeNode page, string value)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.SetValue(DOCUMENT_PAGE_BUILDER_WIDGETS_COLUMN_NAME, value);
        }


        /// <summary>
        /// Gets page template configuration from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to retrieve configuration from.</param>
        /// <returns>Returns page template configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static string GetPageTemplateConfiguration(this TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return page.GetStringValue(DOCUMENT_PAGE_TEMPLATE_CONFIGURATION_COLUMN_NAME, null);
        }


        /// <summary>
        /// Sets page template configuration to <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to set configuration to.</param>
        /// <param name="value">Configuration value to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static void SetPageTemplateConfiguration(this TreeNode page, string value)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.SetValue(DOCUMENT_PAGE_TEMPLATE_CONFIGURATION_COLUMN_NAME, value);
        }


        /// <summary>
        /// Gets A/B test configuration from <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to retrieve configuration from.</param>
        /// <returns>Returns A/B test configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static string GetABTestConfiguration(this TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return page.GetStringValue(DOCUMENT_AB_TEST_CONFIGURATION, null);
        }


        /// <summary>
        /// Sets A/B test configuration to <paramref name="page"/>.
        /// </summary>
        /// <param name="page">Page to set configuration to.</param>
        /// <param name="value">Configuration value to be set.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        public static void SetABTestConfiguration(this TreeNode page, string value)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            page.SetValue(DOCUMENT_AB_TEST_CONFIGURATION, value);
        }
    }
}
