using System.Collections.Generic;
using System.Linq;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Simple implementation of <see cref="ITaxCalculationService"/>. Calculates taxes using tax estimation service.
    /// </summary>
    /// <seealso cref="ITaxEstimationService"/>
    internal class DefaultTaxCalculationService : ITaxCalculationService
    {
        private readonly ITaxEstimationService mEstimationService;
        private readonly ITaxClassService mTaxService;
        private readonly ICustomerTaxClassService mCustomerTaxService;
        private readonly ITaxAddressService mAddressService;
        private readonly ISettingService mSettingService;


        /// <summary>
        /// Gets whether product price already include tax.
        /// </summary>
        private bool IsTaxIncludedInPrice
        {
            get
            {
                return mSettingService.GetBooleanValue(ECommerceSettings.INCLUDE_TAX_IN_PRICES);
            }
        }


        /// <summary>
        /// Constructor. Creates a new instance of <see cref="DefaultTaxCalculationService"/>.
        /// </summary>
        /// <param name="estimationService">Service used to get tax estimates.</param>
        /// <param name="taxService">Service used to get tax classes for items and shipping.</param>
        /// <param name="customerTaxService">Service used to get customer tax class.</param>
        /// <param name="addressService">Service used to get address for tax estimation</param>
        /// <param name="settingService">A setting key service.</param>
        public DefaultTaxCalculationService(ITaxEstimationService estimationService, ITaxClassService taxService, 
            ICustomerTaxClassService customerTaxService, ITaxAddressService addressService, ISettingService settingService)
        {
            mEstimationService = estimationService;
            mTaxService = taxService;
            mCustomerTaxService = customerTaxService;
            mAddressService = addressService;
            mSettingService = settingService;
        }


        /// <summary>
        /// Calculates all the taxes for given purchase.
        /// </summary>
        /// <param name="taxRequest">Tax calculation request containing information about purchase.</param>
        public TaxCalculationResult CalculateTaxes(TaxCalculationRequest taxRequest)
        {
            var customerTaxClass = mCustomerTaxService.GetTaxClass(taxRequest.TaxParameters.Customer);

            // Adds items tax summary and total value to result taxes
            var result = CalculateItemsTax(customerTaxClass, taxRequest.Items, taxRequest.TaxParameters, taxRequest.Discount);

            if (taxRequest.Shipping != null)
            {
                // Calculates shipping tax
                CalculateShippingTax(customerTaxClass, taxRequest, result);
            }

            return result;
        }


        /// <summary>
        /// Calculates the total tax and the tax summary for tax items.
        /// </summary>
        /// <param name="customerTaxClass">Tax class of the customer.</param>
        /// <param name="items">Items to calculate tax for.</param>
        /// <param name="parameters">Parameters of the tax calculation.</param>
        /// <param name="orderDiscount">Discount of the entire order.</param>
        private TaxCalculationResult CalculateItemsTax(CustomerTaxClass customerTaxClass, ICollection<TaxItem> items, TaxCalculationParameters parameters, decimal orderDiscount)
        {
            var result = new TaxCalculationResult();

            var itemsTotal = items.Sum(item => item.Price);

            if (itemsTotal > 0)
            {
                // Calculates tax for items in each tax class
                foreach (var taxClassGroup in items.GroupBy(item => mTaxService.GetTaxClass(item.SKU)))
                {
                    var taxClass = taxClassGroup.Key;
                    var price = GetPriceForTax(taxClassGroup, itemsTotal, orderDiscount);
                    var calculatedTax = CalculateTax(customerTaxClass, taxClass, price, parameters);

                    if (UpdateTaxCalculationResult(result, calculatedTax, taxClass))
                    {
                        result.ItemsTax += calculatedTax;
                    }
                }
            }

            return result;
        }


        private decimal GetPriceForTax(IEnumerable<TaxItem> taxClassItems, decimal totalPrice, decimal discount)
        {
            var priceInClass = taxClassItems.Sum(item => item.Price);
            var proportionalDiscount = priceInClass * discount / totalPrice;
            return priceInClass - proportionalDiscount;
        }


        /// <summary>
        /// Calculates the tax for shipping.
        /// </summary>
        /// <param name="customerTaxClass">Tax class of the customer.</param>
        /// <param name="taxRequest">Tax calculation request.</param>
        /// <param name="taxResult">Tax calculation result</param>
        private void CalculateShippingTax(CustomerTaxClass customerTaxClass, TaxCalculationRequest taxRequest, TaxCalculationResult taxResult)
        {
            // Calculates the shipping tax.
            var taxClass = mTaxService.GetTaxClass(taxRequest.Shipping);
            var calculatedTax = CalculateTax(customerTaxClass, taxClass, taxRequest.ShippingPrice, taxRequest.TaxParameters);

            if (UpdateTaxCalculationResult(taxResult, calculatedTax, taxClass))
            {
                taxResult.ShippingTax = calculatedTax;
            }
        }


        /// <summary>
        /// Adds a tax info to <paramref name="taxCalculationResult"/>.
        /// </summary>
        /// <param name="taxCalculationResult">Result to be updated</param>
        /// <param name="calculatedTax">Tax value to add to <paramref name="taxCalculationResult"/></param>
        /// <param name="taxClass">Tax class to add to <paramref name="taxCalculationResult"/></param>
        /// <remarks>If no tax was calculated, nothing is added to <paramref name="taxCalculationResult"/>.</remarks>
        private bool UpdateTaxCalculationResult(TaxCalculationResult taxCalculationResult, decimal calculatedTax, TaxClassInfo taxClass)
        {
            // If some tax was calculated, the tax info is added into the summary
            if ((taxClass != null) && (calculatedTax != 0m))
            {
                taxCalculationResult.Summary.Sum(taxClass.TaxClassDisplayName, calculatedTax);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Calculates the tax for specified <paramref name="price"/>.
        /// </summary>
        /// <param name="customerTaxClass">Tax class of the customer.</param>
        /// <param name="taxClass">Tax class to be calculated.</param>
        /// <param name="price">Price from which the tax is calculated.</param>
        /// <param name="parameters">Parameters of the tax calculation.</param>
        private decimal CalculateTax(CustomerTaxClass customerTaxClass, TaxClassInfo taxClass, decimal price, TaxCalculationParameters parameters)
        {
            if ((taxClass == null) || customerTaxClass.IsTaxExempt(taxClass))
            {
                return 0m;
            }

            var address = mAddressService.GetTaxAddress(parameters.BillingAddress, parameters.ShippingAddress, taxClass, parameters.Customer);

            var estimationParams = new TaxEstimationParameters
            {
                Currency = parameters.Currency,
                Address = address,
                SiteID = parameters.SiteID
            };

            return IsTaxIncludedInPrice
                ? mEstimationService.ExtractTax(price, taxClass, estimationParams)
                : mEstimationService.GetTax(price, taxClass, estimationParams);
        }
    }
}