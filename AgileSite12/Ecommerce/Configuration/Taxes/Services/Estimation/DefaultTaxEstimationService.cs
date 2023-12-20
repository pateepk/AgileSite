using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides a tax estimation based on country and state. This is the default implementation of the <see cref="ITaxEstimationService"/>. 
    /// </summary>
    public class DefaultTaxEstimationService : ITaxEstimationService
    {
        private readonly ICountryStateTaxRateSource mRateSource;
        private readonly IRoundingServiceFactory mRoundingServiceFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTaxEstimationService"/> class with the specified tax rate source."/> 
        /// </summary>
        /// <param name="rateSource">A source of country/state tax rates.</param>
        /// <param name="roundingServiceFactory">The factory supplying service for price rounding.</param>
        public DefaultTaxEstimationService(ICountryStateTaxRateSource rateSource, IRoundingServiceFactory roundingServiceFactory)
        {
            if (rateSource == null)
            {
                throw new ArgumentNullException(nameof(rateSource));
            }

            if (roundingServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(roundingServiceFactory));
            }

            mRateSource = rateSource;
            mRoundingServiceFactory = roundingServiceFactory;
        }


        /// <summary>
        /// Estimates the tax from the given price.
        /// </summary>
        /// <param name="price">A price without tax.</param>
        /// <param name="taxClass">A tax class for which the taxes are estimated.</param>
        /// <param name="parameters">Parameters of the tax estimation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taxClass"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        public decimal GetTax(decimal price, TaxClassInfo taxClass, TaxEstimationParameters parameters)
        {
            if (taxClass == null)
            {
                throw new ArgumentNullException(nameof(taxClass));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var rate = GetRate(taxClass, parameters.Address);

            return RoundTax(price * rate, parameters);
        }


        /// <summary>
        /// Calculates the tax part of the given price. The result is rounded.
        /// </summary>
        /// <param name="price">A price including tax.</param>
        /// <param name="taxClass">A tax class for which the taxes are estimated.</param>
        /// <param name="parameters">Parameters of the tax estimation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taxClass"/> or <paramref name="parameters"/> is <c>null</c>.</exception>
        public decimal ExtractTax(decimal price, TaxClassInfo taxClass, TaxEstimationParameters parameters)
        {
            if (taxClass == null)
            {
                throw new ArgumentNullException(nameof(taxClass));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var rate = GetRate(taxClass, parameters.Address);
            var extractionRate = GetExtractionRate(rate);

            return RoundTax(price * extractionRate, parameters);
        }


        /// <summary>
        /// Returns a tax rate (0..1) for the specified address (country/state part of the address).
        /// </summary>
        /// <param name="taxClass">Tax class.</param>
        /// <param name="address">Address to get tax rate for.</param>
        private decimal GetRate(TaxClassInfo taxClass, AddressInfo address)
        {
            var countryId = address?.AddressCountryID ?? 0;
            var stateId = address?.AddressStateID ?? 0;

            return mRateSource.GetRate(taxClass.TaxClassID, countryId, stateId);
        }


        /// <summary>
        /// Returns rate for tax extraction.
        /// </summary>
        /// <remarks>
        /// Returned value is not rounded.
        /// </remarks>
        /// <param name="rate">Tax rate (0..1).</param>
        protected virtual decimal GetExtractionRate(decimal rate)
        {
            return rate / (1m + rate);
        }


        /// <summary>
        /// Returns rounded value according to the specified currency.
        /// </summary>
        /// <param name="value">Tax value to be rounded.</param>
        /// <param name="parameters">Currency which supplies number of digits the value is rounded to.</param>
        protected virtual decimal RoundTax(decimal value, TaxEstimationParameters parameters)
        {
            var roundingService = mRoundingServiceFactory.GetRoundingService(parameters.SiteID);
            return roundingService.Round(value, parameters.Currency);
        }
    }
}