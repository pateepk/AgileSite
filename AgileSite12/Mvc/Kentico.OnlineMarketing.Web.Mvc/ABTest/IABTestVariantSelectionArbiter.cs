using System;

using CMS;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.OnlineMarketing;
using CMS.OnlineMarketing.Internal;

using Kentico.OnlineMarketing.Web.Mvc;

[assembly:RegisterImplementation(typeof(IABTestVariantSelectionArbiter), typeof(ABTestVariantSelectionArbiter), Priority = RegistrationPriority.SystemDefault)]

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Defines methods for getting the desired A/B page variant to display to the user.
    /// </summary>
    internal interface IABTestVariantSelectionArbiter
    {
        /// <summary>
        /// Gets A/B variant from the user's cookie or randomly selects one and assigns it to the user. Also checks whether the user should be excluded or not
        /// (see <see cref="ABTestInfo.ABTestIncludedTraffic"/> or <see cref="ABTestInfo.ABTestVisitorTargeting"/>).
        /// </summary>
        /// <param name="page">Page for which to get the A/B test variant.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        ABTestVariant SelectVariant(TreeNode page);


        /// <summary>
        /// Gets A/B variant from the user's cookie or randomly selects one and assigns it to the user. Also checks whether the user should be excluded or not
        /// (see <see cref="ABTestInfo.ABTestIncludedTraffic"/> or <see cref="ABTestInfo.ABTestVisitorTargeting"/>).
        /// </summary>
        /// <param name="page">Page for which to get the A/B test variant.</param>
        /// <param name="variantAssigned">True if variant has been assigned to user, false if it has been loaded from cookie.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is null.</exception>
        ABTestVariant SelectVariant(TreeNode page, out bool variantAssigned);
    }
}
