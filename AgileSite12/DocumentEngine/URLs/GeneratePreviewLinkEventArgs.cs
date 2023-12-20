using System;
using System.Collections.Generic;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Encapsulates event arguments to generate preview link for a <see cref="TreeNode"/>.
    /// </summary>
    /// <seealso cref="PreviewLinkGenerator"/>
    /// <seealso cref="GeneratePreviewLinkHandler"/>
    public class GeneratePreviewLinkEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes an instance of <see cref="GeneratePreviewLinkEventArgs"/>.
        /// </summary>
        /// <param name="page">Defines a <see cref="TreeNode"/> for which the preview link is generated.</param>
        public GeneratePreviewLinkEventArgs(TreeNode page)
        {
            Page = page;
            QueryStringParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns a <see cref="TreeNode"/> for which the preview link is generated.
        /// </summary>
        public TreeNode Page { get; }


        /// <summary>
        /// Defines query string parameters to add the preview link.
        /// </summary>
        public Dictionary<string, string> QueryStringParameters { get; }
    }
}