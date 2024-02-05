namespace CMS.Ecommerce
{
    /// <summary>
    /// Default empty implementation of <see cref="ICalculatorLoggingService"/>.
    /// </summary>
    internal sealed class EmptyCalculatorLoggingService : ICalculatorLoggingService
    {
        /// <summary>
        /// Logs given calculation data.
        /// </summary>
        public void Log(CalculatorData data)
        {
        }
    }
}