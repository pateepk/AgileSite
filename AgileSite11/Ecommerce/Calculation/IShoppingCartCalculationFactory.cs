using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(IShoppingCartCalculationFactory), typeof(ShoppingCartCalculationFactory), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents factory providing shopping cart calculator implementations.
    /// </summary>
    public interface IShoppingCartCalculationFactory
    {
        /// <summary>
        /// Returns calculator implementation based on given <see cref="SiteInfoIdentifier"/>.
        /// </summary>
        /// <remarks>
        /// Never return null object! Use empty implementation if needed.
        /// </remarks>
        IShoppingCartCalculator GetCalculator(SiteInfoIdentifier siteIdentifier);
    }
}
