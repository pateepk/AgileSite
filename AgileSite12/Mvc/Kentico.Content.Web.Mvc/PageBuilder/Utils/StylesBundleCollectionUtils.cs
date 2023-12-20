using System.Web;
using System.Web.Optimization;

using Kentico.Builder.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Contains a utility method for registering page components (inline editors, widgets, sections) styles bundles.
    /// </summary>
    internal static class StylesBundleCollectionUtils
    {
        internal static readonly ComponentStyleBundler bundler = new ComponentStyleBundler();

        private const string INLINE_EDITORS_IDENTIFIER = "InlineEditors";
        private const string SELECTORS_INLINE_EDITORS_IDENTIFIER = "Selectors/InlineEditors";
        private const string WIDGETS_IDENTIFIER = "Widgets";
        private const string SECTIONS_IDENTIFIER = "Sections";


        private static readonly string[] ADMIN_SOURCE_DIRECTORY_VIRTUAL_PATHS =
        {
            // Order is relevant - system styles should be linked before customers' to allow customization
            "~/Kentico/Content/" + INLINE_EDITORS_IDENTIFIER,
            "~/Kentico/Content/" + SELECTORS_INLINE_EDITORS_IDENTIFIER,
            "~/Kentico/Content/" + WIDGETS_IDENTIFIER,
            "~/Kentico/Content/" + SECTIONS_IDENTIFIER,
            "~/Content/" + INLINE_EDITORS_IDENTIFIER,
            "~/Content/" + WIDGETS_IDENTIFIER,
            "~/Content/" + SECTIONS_IDENTIFIER
        };


        private static readonly string[] SOURCE_DIRECTORY_VIRTUAL_PATHS =
        {
            // Order is relevant - system styles should be linked before customers' to allow customization
            "~/Kentico/Content/" + WIDGETS_IDENTIFIER,
            "~/Kentico/Content/" + SECTIONS_IDENTIFIER,
            "~/Content/" + WIDGETS_IDENTIFIER,
            "~/Content/" + SECTIONS_IDENTIFIER
        };


        /// <summary>
        /// Virtual path for the administration UI style bundle.
        /// </summary>
        public const string ADMIN_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/pageComponents/admin/styles";


        /// <summary>
        /// Virtual path for the live site style bundle.
        /// </summary>
        public const string BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/pageComponents/styles";


        /// <summary>
        /// <para>
        /// Registers styles bundle for administration UI to <paramref name="bundles"/>.
        /// </para>
        /// <para>
        /// Styles must reside within '~/Content/InlineEditors' or '~/Content/Widgets' or '~/Content/Sections' directories or theirs subdirectories.
        /// Consider creating a subdirectory named by the corresponding identifier ('CompanyName.ModuleName.InlineEditorName', e.g. 'Kentico.Content.TextEditor').
        /// </para>
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// The registered bundle also contains system specific script files, if any, residing in '~/Kentico/Content/InlineEditors' or
        /// '~/Kentico/Content/Widgets' or '~/Kentico/Content/Sections.
        /// No bundle is registered if any of mentioned directories doesn't exist or doesn't contain any *.css files.
        /// </remarks>
        public static void RegisterPageComponentsAdminStyleBundle(BundleCollection bundles)
        {
            bundler.Register(bundles, ADMIN_BUNDLE_VIRTUAL_PATH, ADMIN_SOURCE_DIRECTORY_VIRTUAL_PATHS);
        }


        /// <summary>
        /// <para>
        /// Registers styles bundle for live site to <paramref name="bundles"/>.
        /// </para>
        /// <para>
        /// Styles must reside within '~/Content/Widgets' or '~/Content/Sections' directories or theirs subdirectories.
        /// Consider creating a subdirectory named by the corresponding identifier ('CompanyName.ModuleName.InlineEditorName', e.g. 'Kentico.Content.TextEditor').
        /// </para>
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// The registered bundle also contains system specific script files, if any, residing in '~/Kentico/Content/Widgets' or '~/Kentico/Content/Sections.
        /// No bundle is registered if any of mentioned directories do not exist.
        /// </remarks>
        public static void RegisterPageComponentsStyleBundle(BundleCollection bundles)
        {
            bundler.Register(bundles, BUNDLE_VIRTUAL_PATH, SOURCE_DIRECTORY_VIRTUAL_PATHS);
        }
    }
}
