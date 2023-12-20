namespace CMS.Ecommerce
{
    /// <summary>
    /// Defines the contract for classes providing tax calculation.
    /// </summary>
    public interface ITaxCalculationService
    {
        /// <summary>
        /// Calculates all the taxes for given purchase.
        /// </summary>
        /// <param name="taxRequest">Tax calculation request containing information about purchase.</param>
        TaxCalculationResult CalculateTaxes(TaxCalculationRequest taxRequest);
    }
}
