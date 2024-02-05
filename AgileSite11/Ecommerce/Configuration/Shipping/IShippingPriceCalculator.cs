namespace CMS.Ecommerce
{
    /// <summary>
    /// Interface for classes providing shipping price calculation.
    /// </summary>
    public interface IShippingPriceCalculator
    {
        /// <summary>
        /// Calculates shipping costs of delivering the <paramref name="delivery"/>.
        /// </summary>
        /// <param name="delivery">Delivery to be checked.</param>
        /// <param name="currencyCode">Code of currency to return shipping price in (e.g. <c>USD</c> or <c>EUR</c>).</param>
        /// <returns>Price in currency specified by currencyCode.</returns>
        decimal GetPrice(Delivery delivery, string currencyCode);
    }
}
