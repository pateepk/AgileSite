using System;
using System.Linq;
using System.Text;

namespace CMS.Ecommerce
{
    /// <summary>
    /// The exception that is thrown when currency conversion fails.
    /// </summary>
    public class InvalidCurrencyConversionException : InvalidOperationException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message</param>
        public InvalidCurrencyConversionException(string message)
            : base(message)
        {
        }
    }
}
