using System;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Holds data required to perform shopping cart calculation.
    /// </summary>
    public sealed class CalculatorData
    {
        /// <summary>
        /// Gets calculation request data.
        /// </summary>
        public CalculationRequest Request
        {
            get;
        }


        /// <summary>
        /// Gets calculation output data.
        /// </summary>
        public CalculationResult Result
        {
            get;
        }


        /// <summary>
        /// Instantiates new instance of <see cref="CalculatorData"/> with given input data <see cref="CalculationRequest"/> and output data <see cref="CalculationResult"/>.
        /// </summary>
        public CalculatorData(CalculationRequest request, CalculationResult result)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Request = request;
            Result = result;
        }
    }
}
