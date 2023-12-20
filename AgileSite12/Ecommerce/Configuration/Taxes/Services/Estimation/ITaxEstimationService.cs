using System;

using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ITaxEstimationService), typeof(DefaultTaxEstimationService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the methods to implement a tax estimation service - i.e. fast, but not so accurate tax calculation.
    /// </summary>
    /// <remarks>
    /// Implementing classes should provide a best effort tax estimation even in cases when some (or all) of the tax estimation parameters are missing.
    /// </remarks>
    /// <seealso cref="TaxEstimationParameters"/>
    /// <seealse cref="ITaxCalculationService"/>
    public interface ITaxEstimationService
    {
        /// <summary>
        /// Estimates the tax from the given price. The result is rounded.
        /// </summary>
        /// <remarks>
        /// This method is used to get taxes for price which does not include tax.
        /// </remarks>
        /// <param name="price">A price without tax.</param>
        /// <param name="taxClass">A tax class for which the taxes are estimated.</param>
        /// <param name="parameters">Parameters of the tax estimation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taxClass"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        decimal GetTax(decimal price, TaxClassInfo taxClass, TaxEstimationParameters parameters);


        /// <summary>
        /// Estimates the tax part of the given price. The result is rounded.
        /// </summary>
        /// <remarks>
        /// This method is used to get (extract) the tax from the price which already includes tax.
        /// </remarks>
        /// <param name="price">A price including tax.</param>
        /// <param name="taxClass">A tax class for which the taxes are estimated.</param>
        /// <param name="parameters">Parameters of the tax estimation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taxClass"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        decimal ExtractTax(decimal price, TaxClassInfo taxClass, TaxEstimationParameters parameters);
    }
}
