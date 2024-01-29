
namespace CMS.Ecommerce
{
    /// <summary>
    /// Represents one part of shopping cart calculation pipeline.
    /// </summary>
    public interface IShoppingCartCalculator
    {
        /// <summary>
        /// Runs shopping cart calculation based on given calculation related data.
        /// </summary>
        /// <param name="calculationData">All calculation related data.</param>
        /// <remarks>
        /// Given <see cref="CalculatorData.Result"/> is modified during calculation process.
        /// </remarks>
        void Calculate(CalculatorData calculationData);
    }
}