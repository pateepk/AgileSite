using System;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.OnlineMarketing.Internal;

using Kentico.OnlineMarketing.Web.Mvc;

[assembly: RegisterImplementation(typeof(IOutputCacheUrlToPageMapper), typeof(OutputCacheUrlToPageMapper), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Maps URL to a <see cref="TreeNode"/> to be used in the output cache.
    /// </summary>
    internal interface IOutputCacheUrlToPageMapper
    {
        /// <summary>
        /// Gets A/B test variant for the page specified by <paramref name="uri"/>.
        /// </summary>
        /// <param name="uri">URL to get variant for.</param>
        /// <returns>A/B test variant if user has a variant assigned, otherwise null.</returns>
        /// <seealso cref="Add(Uri, TreeNode)"/>
        ABTestVariant GetABTestVariantForUrl(Uri uri);


        /// <summary>
        /// Adds a mapping between URL and a page.
        /// </summary>
        /// <param name="url">URL associated with the <paramref name="page"/>.</param>
        /// <param name="page">Page with which to associate the <paramref name="url"/>.</param>
        /// <seealso cref="GetABTestVariantForUrl(Uri)"/>
        void Add(Uri url, TreeNode page);
    }
}