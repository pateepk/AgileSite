using System.Web.Optimization;

using Kentico.Builder.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains a utility method for registering components script bundles.
    /// </summary>
    internal static class ScriptsBundleCollectionUtils
    {
        internal static readonly ComponentScriptBundler bundler = new ComponentScriptBundler();

        public const string JQUERY_SCRIPTS_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/jquery";
        public const string JQUERY_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH = "~/bundles/jquery";

        public const string JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/jquery-unobtrusive-ajax";
        public const string JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_CUSTOM_BUNDLE_VIRTUAL_PATH = "~/bundles/jquery-unobtrusive-ajax";

        private const string FORM_COMPONENTS_IDENTIFIER = "FormComponents";
        private const string FORM_SECTIONS_IDENTIFIER = "FormSections";
        private const string SELECTORS_FORM_COMPONENTS_IDENTIFIER = "Selectors/FormComponents";

        private static readonly string[] SOURCE_DIRECTORY_VIRTUAL_PATHS =
        {
            "~/Kentico/Content/" + FORM_COMPONENTS_IDENTIFIER,
            "~/Kentico/Content/" + SELECTORS_FORM_COMPONENTS_IDENTIFIER,
            "~/Kentico/Content/" + FORM_SECTIONS_IDENTIFIER,
            "~/Content/" + FORM_COMPONENTS_IDENTIFIER,
            "~/Content/" + FORM_SECTIONS_IDENTIFIER,
            "~/Kentico/Scripts/forms",
        };


        /// <summary>
        /// Virtual path for scripts bundle.
        /// </summary>
        public const string BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/forms/scripts";


        /// <summary>
        /// Registers script bundle for forms to <paramref name="bundles"/>.
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// No bundle is registered if there are no script files available.
        /// </remarks>
        public static void RegisterFormsScriptBundle(BundleCollection bundles)
        {
            bundler.Register(bundles, BUNDLE_VIRTUAL_PATH, SOURCE_DIRECTORY_VIRTUAL_PATHS);
        }


        /// <summary>
        /// Registers script bundle for system jQuery to <paramref name="bundles"/>.
        /// </summary>
        public static void RegisterJQueryScriptBundle(BundleCollection bundles)
        {
            var bundle = new ScriptBundle(JQUERY_SCRIPTS_BUNDLE_VIRTUAL_PATH)
                .Include("~/Kentico/Scripts/jquery-3.3.1.js");

            bundles.Add(bundle);
        }


        /// <summary>
        /// Registers script bundle for system jQuery Unobtrusive Ajax to <paramref name="bundles"/>.
        /// </summary>
        public static void RegisterJQueryUnobtrusiveAjaxScriptBundle(BundleCollection bundles)
        {
            var bundle = new ScriptBundle(JQUERY_UNOBTRUSIVE_AJAX_SCRIPTS_BUNDLE_VIRTUAL_PATH)
                .Include("~/Kentico/Scripts/jquery.unobtrusive-ajax.js");

            bundles.Add(bundle);
        }
    }
}
