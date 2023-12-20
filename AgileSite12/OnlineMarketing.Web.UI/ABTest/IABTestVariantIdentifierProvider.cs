using System;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Provides A/B test variant identifier.
    /// </summary>
    internal interface IABTestVariantIdentifierProvider
    {
        /// <summary>
        /// Returns a variant identifer for which the Page builder's configuration is to be saved.
        /// </summary>
        Guid? GetVariantIdentifier();
    }
}