using System;

using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Contains helper method for default A/B test naming.
    /// </summary>
    public class ABTestNameHelper : AbstractHelper<ABTestNameHelper>
    {
        /// <summary>
        /// Gets display name to be used when initializing a new A/B test for a page.
        /// </summary>
        /// <param name="page">Page to get display name for.</param>
        /// <returns>Returns display name for <paramref name="page"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <remarks>
        /// The default implementation returns display name in format '<see cref="TreeNode.DocumentName"/> (<see cref="TreeNode.DocumentCulture"/>)'.
        /// </remarks>
        public static string GetDefaultDisplayName(TreeNode page)
        {
            return HelperObject.GetDefaultDisplayNameInternal(page);
        }


        /// <summary>
        /// Gets display name to be used when initializing a new A/B test for a page.
        /// </summary>
        /// <param name="page">Page to get display name for.</param>
        /// <returns>Returns display name for <paramref name="page"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        /// <remarks>
        /// The default implementation returns display name in format '<see cref="TreeNode.DocumentName"/> (<see cref="TreeNode.DocumentCulture"/>)'.
        /// </remarks>
        protected virtual string GetDefaultDisplayNameInternal(TreeNode page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return $"{page.DocumentName} ({page.DocumentCulture})";
        }
    }
}
