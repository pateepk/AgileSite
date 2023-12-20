using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents parameters of the tax estimation.
    /// </summary>
    /// <seealso cref="ITaxEstimationService"/>
    public class TaxEstimationParameters
    {
        /// <summary>
        /// Gets or sets the address for which the tax is estimated.
        /// </summary>
        public AddressInfo Address
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the currency in which the taxes are calculated.
        /// </summary>
        public CurrencyInfo Currency
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site on which taxes are calculated.
        /// </summary>
        public SiteInfoIdentifier SiteID
        {
            get;
            set;
        }
    }
}