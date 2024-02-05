using CMS;
using CMS.Ecommerce;

[assembly: RegisterImplementation(typeof(ICalculatorLoggingService), typeof(EmptyCalculatorLoggingService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides logging functionality for shopping cart calculation pipeline.
    /// </summary>
    public interface ICalculatorLoggingService
    {
        /// <summary>
        /// Logs given calculation data.
        /// </summary>
        void Log(CalculatorData data);
    }
}