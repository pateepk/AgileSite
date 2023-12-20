using System.Web.Optimization;

using Kentico.Builder.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Contains a utility methods for registering Page builder script's bundles.
    /// </summary>
    internal static class ScriptsBundleCollectionUtils
    {
        internal static readonly ComponentScriptBundler bundler = new ComponentScriptBundler();

        private const string INLINE_EDITORS_IDENTIFIER = "InlineEditors";
        private const string SELECTORS_INLINE_EDITORS_IDENTIFIER = "Selectors/InlineEditors";
        private const string WIDGETS_IDENTIFIER = "Widgets";
        private const string SECTIONS_IDENTIFIER = "Sections";


        private static readonly string[] SOURCE_DIRECTORY_VIRTUAL_PATHS =
        {
            "~/Kentico/Content/" + WIDGETS_IDENTIFIER,
            "~/Kentico/Content/" + SECTIONS_IDENTIFIER,
            "~/Content/" + WIDGETS_IDENTIFIER,
            "~/Content/" + SECTIONS_IDENTIFIER,
        };


        private static readonly string[] ADMIN_SOURCE_DIRECTORY_VIRTUAL_PATHS =
        {
            "~/Kentico/Content/" + INLINE_EDITORS_IDENTIFIER,
            "~/Kentico/Content/" + SELECTORS_INLINE_EDITORS_IDENTIFIER,
            "~/Kentico/Content/" + WIDGETS_IDENTIFIER,
            "~/Kentico/Content/" + SECTIONS_IDENTIFIER,
            "~/Content/" + INLINE_EDITORS_IDENTIFIER,
            "~/Content/" + WIDGETS_IDENTIFIER,
            "~/Content/" + SECTIONS_IDENTIFIER,
        };


        /// <summary>
        /// Virtual path for the live site scripts bundle.
        /// </summary>
        public const string BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/pageComponents/scripts";


        /// <summary>
        /// Virtual path for the administration scripts bundle.
        /// </summary>
        public const string ADMIN_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/pageComponents/admin/scripts";


        /// <summary>
        /// Registers script bundle for page components for a live site to <paramref name="bundles"/>.
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// No bundle is registered if there are no script files available.
        /// </remarks>
        public static void RegisterPageComponentsScriptBundle(BundleCollection bundles)
        {
            bundler.Register(bundles, BUNDLE_VIRTUAL_PATH, SOURCE_DIRECTORY_VIRTUAL_PATHS);
        }


        /// <summary>
        /// Registers script bundle for page components for administration to <paramref name="bundles"/>.
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// No bundle is registered if there are no script files available.
        /// </remarks>
        public static void RegisterPageComponentsAdminScriptBundle(BundleCollection bundles)
        {
            bundler.Register(bundles, ADMIN_BUNDLE_VIRTUAL_PATH, ADMIN_SOURCE_DIRECTORY_VIRTUAL_PATHS);
        }
    }
}
