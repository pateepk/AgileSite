using System.Collections.Generic;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IProductSectionProvider), typeof(ProductSectionProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Describes product section retrieving functionality based on product page hierarchy.
    /// </summary>
    internal interface IProductSectionProvider
    {
        /// <summary>
        /// Returns product sections for combination of product and site.
        /// </summary>
        /// <param name="productID">Product identifier</param>
        IEnumerable<int> GetSections(int productID);
    }
}