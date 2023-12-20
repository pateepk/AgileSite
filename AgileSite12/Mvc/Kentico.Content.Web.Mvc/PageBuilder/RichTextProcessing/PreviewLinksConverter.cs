using System;
using System.Text.RegularExpressions;

using CMS.Helpers;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Converts absolute URL paths to a relative form and removes preview virtual context data in all components properties of the type <see cref="string"/>.
    /// </summary>
    internal class PreviewLinksConverter : IPreviewLinksConverter
    {
        private readonly Regex virtualContextPathPrefixRegex = RegexHelper.GetRegex($@"{VirtualContext.VirtualContextPrefix}.*/{VirtualContext.VirtualContextSeparator}");
        private readonly Regex absolutePathPresentRegex;
        private readonly string applicationPath;


        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewLinksConverter" /> class.
        /// </summary>
        /// <param name="applicationPath">The application path to be unresolved.</param>
        /// <exception cref="ArgumentNullException">applicationPath is <c>null</c></exception>
        public PreviewLinksConverter(string applicationPath)
        {
            this.applicationPath = applicationPath ?? throw new ArgumentNullException(nameof(applicationPath));

            absolutePathPresentRegex = GetAbsolutePathPresentRegex(applicationPath);
        }


        /// <summary>
        /// Unresolves URLs in HTML attributes and removes virtual context data of all the component properties of the type <see cref="string"/>.
        /// </summary>
        /// <param name="configuration">The page builder configuration to be processed.</param>
        /// <exception cref="ArgumentNullException">configuration is <c>null</c></exception>
        public void Unresolve(PageBuilderConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var processor = new ComponentPropertiesProcessor<string>(UnresolveProperty);

            processor.ApplyProcessor(configuration);
        }


        /// <summary>
        /// Unresolves URLs of the given property value if any unresolved URLs found.
        /// </summary>
        private string UnresolveProperty(string value)
        {
            if (ContainsUnresolvedUrls(value))
            {
                // Unresolve all absolute local links and remove virtual context information from the links
                return HTMLHelper.UnResolveUrls(value, applicationPath, RemoveVirtualContextData);
            }

            return value;
        }


        /// <summary>
        /// Gets a primitive regular expression indicating whether a given string contains an absolute path in an HTML attribute.
        /// </summary>
        private Regex GetAbsolutePathPresentRegex(string applicationPath)
        {
            // A naive search for the given application path in:
            //  a) HTML tag: "=..."  =>  href="/applicationPath..."
            //  b) CSS url directive: "(..."  =>  url('/applicationPath...')
            return RegexHelper.GetRegex($@"(=|\()[""']?{applicationPath}",
                                        RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);
        }


        /// <summary>
        /// Indicates whether there is a possibility that the given <paramref name="html"/> will contain unresolved URLs.
        /// </summary>
        /// <remarks>
        /// This method only preforms a simple performance-sensitive heuristic to find absolute paths in the given <paramref name="html"/>.
        /// </remarks>
        internal bool ContainsUnresolvedUrls(string html)
        {
            return !String.IsNullOrEmpty(html) && absolutePathPresentRegex.IsMatch(html);
        }


        /// <summary>
        /// Removes the virtual context data from the given <paramref name="path" />.
        /// </summary>
        /// <remarks>
        /// Removes the virtual context prefix + virtual context query string parameters that are added into a rich text by the <see cref="ContentOutputActionFilter"/>.
        /// In other words transforms "~/cmsctx/-/ABC?uh=123.." into "~/ABC".
        /// </remarks>
        /// <param name="path">The relative path to be modified.</param>
        internal string RemoveVirtualContextData(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }

            if (VirtualContext.ContainsVirtualContextPrefix(path))
            {
                // Remove the virtual context prefix "/cmsctx/.../-"
                path = virtualContextPathPrefixRegex.Replace(path, String.Empty);

                // URLs in a rich text are encoded for security reasons -> decode first before using the "URLHelper.RemoveParametersFromUrl()" method
                // that can work only with raw URLs.
                path = System.Web.HttpUtility.HtmlDecode(path);

                // Remove the virtual context query parameters that were added by the "ContentOutputActionFilter"
                var virtualContextParameterNames = new[] { "uh", "administrationurl" };
                path = URLHelper.RemoveParametersFromUrl(path, virtualContextParameterNames);

                // Encode back the modified URL path
                path = System.Web.HttpUtility.HtmlEncode(path);
            }

            return path;
        }
    }
}
