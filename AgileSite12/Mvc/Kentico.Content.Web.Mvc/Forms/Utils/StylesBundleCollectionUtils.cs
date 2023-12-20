using System;
using System.IO;
using System.Linq;
using System.Web.Optimization;
using System.Web.Hosting;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Contains a utility method for registering components styles bundles.
    /// </summary>
    /// <remarks>
    /// CMS.IO functionality is not supported, implementation uses System.IO deliberately.
    /// </remarks>
    internal static class StylesBundleCollectionUtils
    {
        private const string FORM_COMPONENTS_IDENTIFIER = "FormComponents";
        private const string FORM_SECTIONS_IDENTIFIER = "FormSections";
        private const string SELECTORS_FORM_COMPONENTS_IDENTIFIER = "Selectors/FormComponents";
        private static readonly string[] SOURCE_CSS_DIRECTORY_VIRTUAL_PATHS =
        {
            "~/Content/" + FORM_COMPONENTS_IDENTIFIER,
            "~/Content/" + FORM_SECTIONS_IDENTIFIER
        };
        private static readonly string[] SOURCE_SYSTEM_CSS_DIRECTORY_VIRTUAL_PATHS =
        {
            "~/Kentico/Content/" + FORM_COMPONENTS_IDENTIFIER,
            "~/Kentico/Content/" + SELECTORS_FORM_COMPONENTS_IDENTIFIER,
            "~/Kentico/Content/" + FORM_SECTIONS_IDENTIFIER
        };

        /// <summary>
        /// Virtual path for the administration UI style bundle.
        /// </summary>
        public const string FORM_COMPONENTS_ADMIN_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/formComponents/admin/styles";


        /// <summary>
        /// Virtual path for the live site style bundle.
        /// </summary>
        public const string FORM_COMPONENTS_LIVE_SITE_BUNDLE_VIRTUAL_PATH = "~/kentico/bundles/formComponents/styles";


        /// <summary>
        /// <para>
        /// Registers style bundles for live site and administration UI to <paramref name="bundles"/>.
        /// </para>
        /// <para>
        /// Administration CSS files are expected to end with '.admin.css' suffix, any other '.css' files are considered live site styles.
        /// Both types of files must reside within '~/Content/FormComponents' or '~/Content/FormSections' directories or its subdirectories.
        /// Consider creating a subdirectory named by the corresponding form component's or section's identifier ('CompanyName.ModuleName.ComponentName', e.g. 'Kentico.Content.TextInputComponent').
        /// </para>
        /// </summary>
        /// <param name="bundles">Bundle collection to register the bundles into.</param>
        /// <remarks>
        /// The registered bundle also contains system specific CSS files, if any, residing in '~/Kentico/Content/FormComponents' or '~/Kentico/Content/FormSections'.
        /// No bundle is registered if neither the '~/Content/FormComponents' directory nor the '~/Kentico/Content/FormComponents' directory exist. The same applies to 'FormSections' directories.
        /// </remarks>
        public static void RegisterFormComponentsStyleBundles(BundleCollection bundles)
        {
            var adminBundle = new StyleBundle(FORM_COMPONENTS_ADMIN_BUNDLE_VIRTUAL_PATH);
            var liveSiteBundle = new StyleBundle(FORM_COMPONENTS_LIVE_SITE_BUNDLE_VIRTUAL_PATH);
            bool includeAdminBundle = false;
            bool includeLiveSiteBundle = false;

            foreach (var sourceVirtualPath in new [] { SOURCE_SYSTEM_CSS_DIRECTORY_VIRTUAL_PATHS, SOURCE_CSS_DIRECTORY_VIRTUAL_PATHS }.SelectMany(path => path))
            {
                var sourceDirectoryPath = HostingEnvironment.MapPath(sourceVirtualPath);
#pragma warning disable BH1014 // Do not use System.IO
                if (!Directory.Exists(sourceDirectoryPath))
#pragma warning restore BH1014 // Do not use System.IO
                {
                    continue;
                }

                adminBundle.IncludeDirectory(sourceVirtualPath, "*.css", true);
                includeAdminBundle = true;

#pragma warning disable BH1014 // Do not use System.IO
                var liveSiteFiles = Directory.EnumerateFiles(sourceDirectoryPath, "*.css", SearchOption.AllDirectories)
#pragma warning restore BH1014 // Do not use System.IO
                                                                            .Where(p => !p.EndsWith(".admin.css", StringComparison.OrdinalIgnoreCase))
                                                                            .Select(URLHelper.UnMapPath)
                                                                            .ToArray();
                if (liveSiteFiles.Length == 0)
                {
                    continue;
                }

                liveSiteBundle.Include(liveSiteFiles);
                includeLiveSiteBundle = true;
            }

            if (includeAdminBundle)
            {
                bundles.Add(adminBundle);
            }
            if (includeLiveSiteBundle)
            {
                bundles.Add(liveSiteBundle);
            }
        }
    }
}
